using System;
using System.Collections.Generic;
using UnityEngine;

namespace SepCore.Exploration
{
    [Serializable]
    public struct ResourcePointDefinition
    {
        [SerializeField] private Vector2 _position;
        [SerializeField] private int _resourcePointId;

        public Vector2 Position => _position;

        public int ResourcePointId => _resourcePointId;
    }

    [CreateAssetMenu(fileName = "MapDefinition", menuName = "SBE/Map Definition")]
    public sealed class MapDefinition : ScriptableObject
    {
        [SerializeField] private List<Vector2> _playerSpawnPoints = new List<Vector2>();
        [SerializeField] private List<ResourcePointDefinition> _resourcePoints = new List<ResourcePointDefinition>();
        [SerializeField] private List<Vector2> _enemySpawnPoints = new List<Vector2>();
        [SerializeField] private List<Vector2> _extractionPoints = new List<Vector2>();

        public IReadOnlyList<Vector2> PlayerSpawnPoints => _playerSpawnPoints;

        public IReadOnlyList<ResourcePointDefinition> ResourcePoints => _resourcePoints;

        public IReadOnlyList<Vector2> EnemySpawnPoints => _enemySpawnPoints;

        public IReadOnlyList<Vector2> ExtractionPoints => _extractionPoints;
    }
}
