using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ZombleMode
{
    public static class Config
    {
        #region Paths

        private static readonly string ConfigDir = Path.Combine(TShock.SavePath, "ZombleMode");
        private static readonly string PlayersPath = Path.Combine(ConfigDir, "players.json");
        private static readonly string RoomsPath = Path.Combine(ConfigDir, "rooms.json");
        private static readonly string PacksPath = Path.Combine(ConfigDir, "packs.json");

        #endregion

        #region Data

        private static List<ZPlayer> _players = new List<ZPlayer>();
        private static List<ZRoom> _rooms = new List<ZRoom>();
        private static List<MiniPack> _packs = new List<MiniPack>();

        #endregion

        #region Methods

        public static void Initialize()
        {
            EnsureDirectoryExists(ConfigDir);

            LoadPlayers();
            LoadRooms();
            LoadPacks();
        }

        public static void SavePlayers()
        {
            SaveData(_players, PlayersPath);
        }

        public static void SaveRooms()
        {
            SaveData(_rooms, RoomsPath);
        }

        public static void SavePacks()
        {
            SaveData(_packs, PacksPath);
        }

        public static ZPlayer GetPlayerByName(string name)
        {
            return _players.SingleOrDefault(p => p.Name == name);
        }

        public static ZRoom GetRoomById(int id)
        {
            return _rooms.SingleOrDefault(p => p.ID == id);
        }

        public static MiniPack GetPackById(int id)
        {
            return _packs.SingleOrDefault(p => p.ID == id);
        }

        public static void UpdateSingleRoom(ZRoom room)
        {
            var existingRoom = _rooms.SingleOrDefault(r => r.ID == room.ID);

            if (existingRoom != null)
            {
                _rooms.Remove(existingRoom);
            }

            _rooms.Add(room);
            SaveRooms();
        }

        public static void AddSingleRoom(ZRoom room)
        {
            _rooms.Add(room);
            SaveRooms();
        }

        public static void RemoveSingleRoom(ZRoom room)
        {
            _rooms.Remove(room);
            SaveRooms();
        }

        public static void ReloadPacks()
        {
            LoadPacks();
        }

        #endregion

        #region Helper Methods

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void LoadData<T>(List<T> list, string filePath)
        {
            if (File.Exists(filePath))
            {
                list = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filePath));
            }
        }

        private static void SaveData<T>(List<T> list, string filePath)
        {
            var serializedData = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(filePath, serializedData);
        }

        #endregion
    }
}
