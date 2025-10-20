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
        
        private void OnGridClicked(int x, int y)
        {
            var relatedGrid = _gridMap[x, y];
            _onGridClicked?.Invoke(relatedGrid);
            
            Debug.Log($"Grid clicked at ({x},{y})");
        }
    }
}
