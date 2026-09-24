using System;
using Core;

namespace View
{
    /// <summary>What every view receives from GameBootstrap: the live model plus art and colors for its content.</summary>
    public sealed class GameContext
    {
        public GameModel Model { get; private set; }
        public ContentLookup Content { get; private set; }

        public GameContext(GameModel model, ContentLookup content)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }
    }
}
