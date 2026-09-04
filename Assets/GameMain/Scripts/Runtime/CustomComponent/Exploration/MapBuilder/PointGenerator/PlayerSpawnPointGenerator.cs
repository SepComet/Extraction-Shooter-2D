using System;
using System.Collections.Generic;
using SepCore.CustomComponent;
using UnityEngine;

namespace SepCore.Exploration
{
    internal sealed class PlayerSpawnPointGenerator
    {
        private readonly IRunRandomSource _random;

        public PlayerSpawnPointGenerator(IRunRandomSource random)
        {
            _random = random;
        }

        public Vector2 Generate(IReadOnlyList<Vector2> definitions)
        {
            if (definitions.Count == 0)
            {
                throw new InvalidOperationException("Map definition does not contain a player spawn point.");
            }

            return definitions[_random.NextInt(0, definitions.Count)];
        }
    }
}
