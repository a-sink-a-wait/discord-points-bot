using System;
using System.IO;

namespace Durable
{
    public class WarId
    {
        private readonly string _warId;
        private readonly string[] _splitId;

        private WarId(string warId)
        {
            _splitId = warId.Split("_");
            _warId = warId;
        }

        public static bool TryParse(string warId, out string id)
        {
            if (warId.Split("_").Length != 2)
            {
                id = null;
                return false;
            }

            id = new WarId(warId);
            return true;
        }

        public bool EqualsId(WarId other)
        {
            return String.Equals(this, other, StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(Swapped(), other, StringComparison.OrdinalIgnoreCase);
        }

        private WarId Swapped() => $"{this._splitId[1]}_{this._splitId[0]}";

        public static implicit operator string(WarId id) => id._warId;

        public static implicit operator string[](WarId id) => id._splitId;

        public static implicit operator WarId(string[] warId) => new WarId(String.Join('_', warId));

        public static implicit operator WarId(string warId) => new WarId(warId);
    }
}