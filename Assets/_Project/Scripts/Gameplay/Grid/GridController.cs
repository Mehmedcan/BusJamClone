using System;
using System.Collections.Generic;
using _Project.Data.Constants;
using _Project.Data.GameData;
using _Project.Scripts.Core;
using _Project.Scripts.Systems.Pooling;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Grid
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private Transform firstGridTransform;
        [SerializeField] private Grid gridPrefab;

        private Action<Grid> _onGridClicked;
        private Grid[,] _gridMap;
        
        private List<GameObject> _pooledStickmen = new();
        private IPoolManager _poolManager;
        
        public void CreateGridBoard(Vector2Int size, List<GridCellConfig> girdCellConfigList, float padding)
        {
            Locator.Instance.TryResolve(out _poolManager);
            _gridMap ??= new Grid[size.x, size.y];

            var gridIndex = 0;
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var position = new Vector3(
                        firstGridTransform.position.x + (x * (gridPrefab.transform.localScale.x + padding)),
                        firstGridTransform.position.y,
                        firstGridTransform.position.z - (y * (gridPrefab.transform.localScale.z + padding))
                    );
            
                    var gridCellConfig = girdCellConfigList[gridIndex];
                    var newGrid = Instantiate(gridPrefab, position, Quaternion.identity, transform);
                    newGrid.Initialize(gridCellConfig, OnGridClicked, GetStickmanFromPool);
            
                    newGrid.name = $"GridCell ({x},{y}) | Type: {gridCellConfig.gridType} | Human: {gridCellConfig.humanType}";
                    _gridMap[x, y] = newGrid;
                    gridIndex++;
                }
            }
        }
        
        public void SetGridClick(Action<Grid> onGridClicked)
        {
            _onGridClicked = onGridClicked;
        }

        public bool CanStickmanExit(Grid grid)
        {
            // already top
            if (grid.Coordinates.y == 0)
            {
                return true;
            }

            var rows = _gridMap.GetLength(0);
            var cols = _gridMap.GetLength(1);

            // bfs init
            var visited = new bool[rows, cols];
            var queue = new Queue<Vector2Int>();
            
            // start
            queue.Enqueue(grid.Coordinates);
            visited[grid.Coordinates.x, grid.Coordinates.y] = true;

            Vector2Int[] directions = 
            {
                new Vector2Int(0, -1),
                new Vector2Int(0, 1),
                new Vector2Int(-1, 0),
                new Vector2Int(1, 0)
            };

            while (queue.Count > 0)
            {
                var currentPos = queue.Dequeue();
                
                foreach (var direction in directions)
                {
                    var neighborPos = currentPos + direction;
                    
                    if (neighborPos.x >= 0 && neighborPos.x < rows && 
                        neighborPos.y >= 0 && neighborPos.y < cols)
                    {
                        // skip if already visited
                        if (visited[neighborPos.x, neighborPos.y])
                            continue;
                            
                        var neighborGrid = _gridMap[neighborPos.x, neighborPos.y];
                        
                        if (neighborGrid.Type == GridType.Empty || neighborGrid.Type == GridType.Processing)
                        {
                            // If reached y=0, exit
                            if (neighborPos.y == 0)
                            {
                                return true;
                            }
                            
                            // mark as visited
                            visited[neighborPos.x, neighborPos.y] = true;
                            queue.Enqueue(neighborPos);
                        }
                    }
                }
            }

            return false;
        }
        
        private void OnGridClicked(int x, int y)
        {
            var relatedGrid = _gridMap[x, y];
            _onGridClicked?.Invoke(relatedGrid);
        }


        public void ReturnPoolObjects()
        {
            if (_poolManager != null && _pooledStickmen.Count > 0)
            {
                _poolManager.ReturnToPool(GameConstants.STICKMAN_POOL_KEY, _pooledStickmen);
                _pooledStickmen.Clear();   
            }
        }
        private GameObject GetStickmanFromPool(Transform parent)
        {
            var stickman = _poolManager.Get(GameConstants.STICKMAN_POOL_KEY, parent);
            
            _pooledStickmen.Add(stickman);
            return stickman;
        }
    }
}
