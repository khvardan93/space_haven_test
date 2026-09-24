using System;
using System.Collections.Generic;
using Configs;
using Offline;
using Save;
using View;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Composition root and the only MonoBehaviour that owns the model.
    /// Startup order matters: load save, apply offline earnings, then create views,
    /// so catch-up production does not trigger per-cycle UI effects.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const string SaveFileName = "space_prison_save.json";

        [SerializeField] private GameConfigs _config;

        [Header("Views")]
        [SerializeField] private PrisonView _prisonView;
        [SerializeField] private RoomActionPopup _actionPopup;

        [Header("Saving")]
        [SerializeField] private bool _useSave = true;
        [SerializeField, Min(5f)] private float _autosaveSeconds = 30f;

        [Header("Mobile")]
        [SerializeField] private int _targetFrameRate = 60;

        private GameModel _model;
        private GameContext _context;
        private JsonSaveStorage _storage;
        private OfflineEarnings _offline;
        private float _autosaveTimer;
        private DateTime _pausedAtUtc;
        private bool _paused;

        public GameContext Context
        {
            get { return _context; }
        }

        /// <summary>Last offline catch-up result (launch or return from background). Null when nothing was applied.</summary>
        public OfflineReport LastOfflineReport { get; private set; }

        /// <summary>Raised when time away was converted into resources. Phase 7 shows the welcome-back popup from this.</summary>
        public event Action<OfflineReport> OfflineEarningsApplied;

        private void Awake()
        {
            Application.targetFrameRate = _targetFrameRate;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;

            if (_config == null)
            {
                Debug.LogError("[Bootstrap] GameConfig is not assigned.", this);
                enabled = false;
                return;
            }

            var setup = _config.ToSetup();
            _storage = new JsonSaveStorage(SaveFileName);
            _offline = new OfflineEarnings(new SystemTimeProvider(), setup.OfflineCap);

            GameState state;
            if (_useSave && _storage.TryLoad(out state))
            {
                List<string> warnings;
                _model = GameModel.FromSave(setup, state, out warnings);
                foreach (var warning in warnings)
                    Debug.LogWarning("[Save] " + warning);

                LastOfflineReport = _offline.Apply(state.LastSaveUtc, _model.Simulation, _model.Economy);
            }
            else
            {
                _model = GameModel.CreateNew(setup);
            }

            _context = new GameContext(_model, new ContentLookup(_config));

            _actionPopup.Init(_context);
            _prisonView.Init(_context);
            _prisonView.SlotClicked += _actionPopup.Open;
        }

        private void Start()
        {
            // Listeners from other components subscribe in their Awake/OnEnable, so raise the launch report here.
            if (LastOfflineReport != null && LastOfflineReport.HasGains)
                RaiseOfflineApplied(LastOfflineReport);
        }

        private void Update()
        {
            if (_model == null || _paused)
                return;

            _model.Simulation.Update(Time.unscaledDeltaTime);

            _autosaveTimer += Time.unscaledDeltaTime;
            if (_autosaveTimer >= _autosaveSeconds)
            {
                _autosaveTimer = 0;
                Save();
            }
        }

        /// <summary>
        /// Android sends the app to background with pause=true. Save immediately (the OS may kill us without OnApplicationQuit),
        /// and on return convert the time away into offline earnings instead of one huge frame.
        /// </summary>
        private void OnApplicationPause(bool pause)
        {
            if (_model == null)
                return;

            if (pause)
            {
                _paused = true;
                _pausedAtUtc = DateTime.UtcNow;
                Save();
            }
            else if (_paused)
            {
                _paused = false;
                var report = _offline.Apply(_pausedAtUtc, _model.Simulation, _model.Economy);
                LastOfflineReport = report;
                if (report.HasGains)
                    RaiseOfflineApplied(report);
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void OnDestroy()
        {
            if (_prisonView != null && _actionPopup != null)
                _prisonView.SlotClicked -= _actionPopup.Open;
            if (_model != null)
                _model.Dispose();
        }

        public void Save()
        {
            if (_model == null || !_useSave)
                return;

            _storage.Save(SaveMapper.Capture(_model, DateTime.UtcNow));
        }

        /// <summary>Demo helper for the x1 / x5 buttons (wired in Phase 7).</summary>
        public void SetTimeScale(float scale)
        {
            if (_model != null)
                _model.Simulation.TimeScale = scale;
        }

        [ContextMenu("Delete Save")]
        private void DeleteSave()
        {
            new JsonSaveStorage(SaveFileName).Delete();
            _useSave = false; // Prevent OnApplicationQuit from writing it back during this play session.
            Debug.Log("[Save] Deleted. Restart play mode for a fresh game.");
        }

        private void RaiseOfflineApplied(OfflineReport report)
        {
            Debug.Log("[Offline] " + NumberFormat.Duration(report.Elapsed.TotalSeconds) + (report.WasCapped ? " (capped)" : ""));
            var handler = OfflineEarningsApplied;
            if (handler != null) handler(report);
        }
    }
}
