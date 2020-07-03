using System;
using System.IO;

namespace Durable.Entity
{
    public class WarId
    {
        private readonly string _warId;
        private readonly string[] _splitId;

        public WarId(string warId)
        {
            _splitId = warId.Split("_").Length == 2 ? warId.Split("_") : throw new InvalidDataException($"Invalid WarId: {warId}");
            _warId = warId;
        }

        public bool EqualsId(WarId other)
        {
            return String.Equals(this, other, StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(Swapped(), other, StringComparison.OrdinalIgnoreCase);
        }

        public WarId Swapped() => $"{this._splitId[1]}_{this._splitId[0]}";

        public static implicit operator string(WarId id) => id._warId;

        public static implicit operator string[](WarId id) => id._splitId;

        public static implicit operator WarId(string[] warId) => new WarId(String.Join('_', warId));

        public static implicit operator WarId(string warId) => new WarId(warId);

        public override string ToString() => $"{_warId}";
    }
}