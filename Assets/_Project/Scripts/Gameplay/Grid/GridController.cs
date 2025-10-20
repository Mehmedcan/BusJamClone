using System;
using System.Collections.Generic;
using _Project.Data.GameData;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Grid
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private Transform firstGridTransform;
        [SerializeField] private Grid gridPrefab;

        private Action<Grid> _onGridClicked;
        private Grid[,] _gridMap;
        
        public void CreateGridBoard(Vector2Int size, List<GridCellConfig> girdCellConfigList, float padding)
        {
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
                    newGrid.Initialize(gridCellConfig, OnGridClicked);
            
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
            if (grid.Coordinates.y == 0)
            {
                return true;
            }

            var rows = _gridMap.GetLength(0);
            var cols = _gridMap.GetLength(1);

            Vector2Int[] directions = 
            {
                new Vector2Int(0, 1),  // top
                new Vector2Int(0, -1), // bottom
                new Vector2Int(-1, 0), // left
                new Vector2Int(1, 0)   // right
            };

            foreach (var direction in directions)
            {
                var neighborCoordinate = grid.Coordinates + direction;

                if (neighborCoordinate.x >= 0 && neighborCoordinate.x < cols &&
                    neighborCoordinate.y >= 0 && neighborCoordinate.y < rows)
                {
                    var neighborGrid = _gridMap[neighborCoordinate.x, neighborCoordinate.y];
                    if (neighborGrid.Type == GridType.Empty || neighborGrid.Type == GridType.Processing)
                    {
                        return true;
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
    }
}
