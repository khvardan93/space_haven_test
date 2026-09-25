using System.Collections.Generic;
using System.Text;
using Configs;
using Economy;
using UnityEngine;

namespace View
{
    
    public static class CostFormatter
    {
        private static readonly StringBuilder Builder = new(64);

        public static string Cost(IReadOnlyList<Resource> cost, int laserCost, GameContext context)
        {
            Builder.Length = 0;
            var economy = context.Model.Economy;

            if (cost != null)
            {
                for (var i = 0; i < cost.Count; i++)
                {
                    var affordable = economy.Get(cost[i].TypeEnum) + 1e-9 >= cost[i].Amount;
                    AppendAmount(cost[i], context, affordable ? null : UiColors.MissingHex);
                }
            }

            if (laserCost > 0)
            {
                var affordable = context.Model.Laser.CanSpend(laserCost);
                Separator();
                Builder.Append("<color=").Append(affordable ? UiColors.LaserHex : UiColors.MissingHex).Append('>')
                       .Append(laserCost).Append(' ').Append("LE").Append("</color>");
            }

            return Builder.ToString();
        }

        public static string Production(RoomConfigs spec, IReadOnlyList<Resource> outputs, GameContext context)
        {
            Builder.Length = 0;

            if (spec.Inputs.Count > 0)
            {
                for (var i = 0; i < spec.Inputs.Count; i++)
                {
                    if (i > 0) Builder.Append(" + ");
                    AppendAmount(spec.Inputs[i], context, null, false);
                }
                Builder.Append("  >  ");
            }

            for (var i = 0; i < outputs.Count; i++)
            {
                if (i > 0) Builder.Append(" + ");
                AppendAmount(outputs[i], context, null, false);
            }

            Builder.Append(" / ").Append(NumberFormat.Duration(spec.CycleTime));
            return Builder.ToString();
        }

        public static string Hex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        private static void AppendAmount(Resource amount, GameContext context, string overrideColor, bool separate = true)
        {
            if (separate) Separator();

            var color = overrideColor ?? Hex(context.Content.GetColor(amount.TypeEnum));
            Builder.Append("<color=").Append(color).Append('>')
                   .Append(NumberFormat.Short(amount.Amount)).Append(' ')
                   .Append(context.Content.GetShortName(amount.TypeEnum))
                   .Append("</color>");
        }

        private static void Separator()
        {
            if (Builder.Length > 0)
                Builder.Append("   ");
        }
    }
}
