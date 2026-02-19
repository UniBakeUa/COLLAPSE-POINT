using System.Collections.Generic;
using System.Linq;
using _Game.Content.Features.BuildingModule.Scripts.RoomSetup;
using _Game.Content.Features.BuildingModule.Scripts.Supports;
using UnityEngine;

namespace _Game.Content.Features.BuildingModule.Scripts
{
    public class WeightCalculator
    {
        private readonly RoomsConfig _roomsConfig;
        private readonly SupportsConfig _supportsConfig;

        public WeightCalculator(RoomsConfig roomsConfig, SupportsConfig supportsConfig)
        {
            _roomsConfig = roomsConfig;
            _supportsConfig = supportsConfig;
        }

        public void RecalculatePhysics(
            List<RoomData> allRooms, 
            Dictionary<int, SupportData> activeSupports,
            RoomGrid grid)
        {
            if (allRooms == null || allRooms.Count == 0) return;

            // 1. Скидаємо накопичене навантаження перед новим розрахунком
            foreach (var r in allRooms) r.ReceivedLoad = 0;
            grid.ResetLoads(); 

            // 2. Сортуємо кімнати зверху вниз
            var sortedRooms = allRooms.OrderByDescending(r => r.GridPosition.y).ToList();

            foreach (var room in sortedRooms)
            {
                var roomStats = _roomsConfig.GetLevel(room.Difficulty);
                
                // Власна вага + те, що прийшло зверху
                room.SelfWeight = (room.Size.x * room.Size.y) * roomStats.weightPerCell;

                // Перевірка на краш кімнати
                if (room.ReceivedLoad > roomStats.maxIncomingLoad)
                {
                    HandleRoomCollapse(room);
                    continue;
                }

                // 3. Розподіл ваги
                if (room.AttachedSupportIds.Count > 0)
                {
                    // Якщо є балки, вони ділять всю вагу кімнати між собою
                    float loadPerSupport = room.TotalWeight / room.AttachedSupportIds.Count;

                    foreach (var supportId in room.AttachedSupportIds)
                    {
                        if (activeSupports.TryGetValue(supportId, out var support))
                        {
                            // Перевірка стабільності самої балки
                            if (!CheckSupportStability(support, loadPerSupport, grid))
                            {
                                // Якщо балка зламалася, вона не передає вагу далі
                                continue; 
                            }

                            // Передаємо вагу в точку End
                            TransferLoadFromSupport(support, loadPerSupport, allRooms, grid);
                        }
                    }
                }
                else
                {
                    // Якщо балок немає, вага тисне просто вниз на сусідні клітинки
                    PassWeightDirectlyBelow(room, room.TotalWeight, allRooms, grid);
                }
            }
        }

        private void TransferLoadFromSupport(SupportData support, float load, List<RoomData> allRooms, RoomGrid grid)
        {
            // Визначаємо, куди впирається кінець балки
            Vector2Int endCoord = new Vector2Int(Mathf.FloorToInt(support.End.x), Mathf.FloorToInt(support.End.y));
            var cell = grid.GetCell(endCoord);

            // Якщо влучили в іншу кімнату
            if (cell?.Type == RoomGrid.CellType.Room)
            {
                var targetRoom = allRooms.Find(r => r.Id == cell?.OwnerId);
                if (targetRoom != null)
                {
                    targetRoom.ReceivedLoad += load;
                }
            }
            
            // Записуємо навантаження в сітку для візуалізації (якщо потрібно)
            grid.AddLoad(endCoord, load);
        }

        private void PassWeightDirectlyBelow(RoomData room, float weight, List<RoomData> allRooms, RoomGrid grid)
        {
            float weightPerColumn = weight / room.Size.x;
            for (int x = 0; x < room.Size.x; x++)
            {
                Vector2Int cellBelow = new Vector2Int(room.GridPosition.x + x, room.GridPosition.y - 1);
                var cell = grid.GetCell(cellBelow);

                if (cell?.Type == RoomGrid.CellType.Room)
                {
                    var targetRoom = allRooms.Find(r => r.Id == cell?.OwnerId);
                    if (targetRoom != null) targetRoom.ReceivedLoad += weightPerColumn;
                }
                
                grid.AddLoad(cellBelow, weightPerColumn);
            }
        }

        private bool CheckSupportStability(SupportData support, float load, RoomGrid grid)
        {
            var material = _supportsConfig.GetLevel(0); // Можна розширити до materialLevel балки

            // Розрахунок кута нахилу (90 - вертикаль, 0 - горизонталь)
            Vector2 dir = (support.End - support.Start).normalized;
            float angle = Mathf.Abs(Mathf.Asin(dir.y) * Mathf.Rad2Deg);

            // Визначаємо ліміт залежно від кута (як ти і просив < 50)
            float limit = (angle < 50f) 
                ? material.maxLoad * _supportsConfig.horizontalWeightEfficiency 
                : material.maxLoad;

            if (load > limit)
            {
                Debug.LogWarning($"[Physics] Support {support.Id} collapsed! Angle: {angle:F1} Load: {load:F1} > Limit: {limit:F1}");
                // Тут треба викликати видалення через Generator
                return false;
            }

            return true;
        }

        private void HandleRoomCollapse(RoomData room)
        {
            Debug.LogError($"[Physics] ROOM {room.Id} CRUSHED! Incoming Load: {room.ReceivedLoad}");
            // Подія руйнування кімнати
        }
    }
}