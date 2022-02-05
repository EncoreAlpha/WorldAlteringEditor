﻿using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// Defines types that can be copied from the map.
    /// </summary>
    [Flags]
    public enum CopiedEntryType
    {
        Invalid = 0,
        Terrain = 1,
        Overlay = 2,
        TerrainObject = 4,
        Structure = 8,
        Vehicle = 16,
        Infantry = 32
    }

    public class CopiedMapDataSerializationException : Exception
    {
        public CopiedMapDataSerializationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Base class for all objects that can be copied from a map.
    /// </summary>
    public abstract class CopiedMapEntry
    {
        public Point2D Offset { get; protected set; }
        public abstract CopiedEntryType EntryType { get; }

        private byte[] buffer;

        protected CopiedMapEntry()
        {
        }

        protected CopiedMapEntry(Point2D offset)
        {
            Offset = offset;
        }

        protected int ReadInt(Stream stream)
        {
            if (stream.Read(buffer, 0, 4) != 4)
                throw new CopiedMapDataSerializationException("Failed to read integer from stream: end of stream");

            return BitConverter.ToInt32(buffer, 0);
        }

        protected string ReadASCIIString(Stream stream)
        {
            int length = ReadInt(stream);
            byte[] stringBuffer = new byte[length];
            if (stream.Read(stringBuffer, 0, length) != length)
                throw new CopiedMapDataSerializationException("Failed to read string from stream: end of stream");

            string result = Encoding.ASCII.GetString(stringBuffer);
            return result;
        }

        protected byte[] ASCIIStringToBytes(string str)
        {
            byte[] buffer = new byte[sizeof(int) + str.Length];
            Array.Copy(BitConverter.GetBytes(str.Length), buffer, sizeof(int));
            byte[] stringBytes = Encoding.ASCII.GetBytes(str);
            Array.Copy(stringBytes, 0, buffer, sizeof(int), stringBytes.Length);
            return buffer;
        }

        /// <summary>
        /// Reads all of the map entry's data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the data from.</param>
        public void ReadData(Stream stream)
        {
            buffer = new byte[8];

            int x = ReadInt(stream);
            int y = ReadInt(stream);
            Offset = new Point2D(x, y);
            ReadCustomData(stream);
            buffer = null; // Free memory
        }

        /// <summary>
        /// When overriden in a derived class, reads and applies this instance's custom data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the custom data from.</param>
        protected abstract void ReadCustomData(Stream stream);

        /// <summary>
        /// Writes all of the map entry's data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the data to.</param>
        public void WriteData(Stream stream)
        {
            stream.WriteByte((byte)EntryType);
            byte[] offsetData = Offset.GetData();
            stream.Write(offsetData, 0, offsetData.Length);

            byte[] data = GetCustomData();
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// When overriden in a derived class, returns this instance's custom data serialized to an array of bytes.
        /// </summary>
        protected abstract byte[] GetCustomData();
    }

    public class CopiedTerrainEntry : CopiedMapEntry
    {
        public int TileIndex;
        public byte SubTileIndex;

        public CopiedTerrainEntry()
        {
        }

        public CopiedTerrainEntry(Point2D offset, int tileIndex, byte subTileIndex) : base(offset)
        {
            TileIndex = tileIndex;
            SubTileIndex = subTileIndex;
        }

        public override CopiedEntryType EntryType => CopiedEntryType.Terrain;

        protected override byte[] GetCustomData()
        {
            byte[] returnValue = new byte[5];
            Array.Copy(BitConverter.GetBytes(TileIndex), returnValue, 4);
            returnValue[4] = SubTileIndex;

            return returnValue;
        }

        protected override void ReadCustomData(Stream stream)
        {
            TileIndex = ReadInt(stream);
            SubTileIndex = (byte)stream.ReadByte();
        }
    }

    public class CopiedOverlayEntry : CopiedMapEntry
    {
        public string OverlayTypeName;
        public int FrameIndex;

        public CopiedOverlayEntry()
        {
        }

        public CopiedOverlayEntry(Point2D offset, string overlayTypeName, int frameIndex) : base(offset)
        {
            OverlayTypeName = overlayTypeName;
            FrameIndex = frameIndex;
        }

        public override CopiedEntryType EntryType => CopiedEntryType.Overlay;

        protected override byte[] GetCustomData()
        {
            byte[] nameBytes = ASCIIStringToBytes(OverlayTypeName);
            byte[] buffer = new byte[ nameBytes.Length + sizeof(int)];
            Array.Copy(nameBytes, buffer, nameBytes.Length);
            Array.Copy(BitConverter.GetBytes(FrameIndex), 0, buffer, nameBytes.Length, sizeof(int));
            return buffer;
        }

        protected override void ReadCustomData(Stream stream)
        {
            OverlayTypeName = ReadASCIIString(stream);
            FrameIndex = ReadInt(stream);
        }
    }

    public class CopiedTerrainObjectEntry : CopiedMapEntry
    {
        public string TerrainObjectTypeName;

        public CopiedTerrainObjectEntry()
        {
        }

        public CopiedTerrainObjectEntry(Point2D offset, string terrainObjectTypeName) : base(offset)
        {
            TerrainObjectTypeName = terrainObjectTypeName;
        }

        public override CopiedEntryType EntryType => CopiedEntryType.TerrainObject;

        protected override byte[] GetCustomData()
        {
            return ASCIIStringToBytes(TerrainObjectTypeName);
        }

        protected override void ReadCustomData(Stream stream)
        {
            TerrainObjectTypeName = ReadASCIIString(stream);
        }
    }

    public class CopiedMapData
    {
        public List<CopiedMapEntry> CopiedMapEntries { get; set; } = new List<CopiedMapEntry>();

        public byte[] Serialize()
        {
            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                // Write entry count
                memoryStream.Write(BitConverter.GetBytes(CopiedMapEntries.Count), 0, sizeof(int));

                // Write entries
                foreach (var entry in CopiedMapEntries)
                {
                    entry.WriteData(memoryStream);
                }

                bytes = memoryStream.ToArray();
            }

            return bytes;
        }

        public void Deserialize(byte[] bytes)
        {
            if (bytes.Length < 4)
            {
                Logger.Log("Failed to deserialize copied map data: provided array is less than 4 bytes in size.");
                return;
            }

            int entryCount = BitConverter.ToInt32(bytes, 0);

            using (var memoryStream = new MemoryStream(bytes))
            {
                memoryStream.Position = 4;

                for (int i = 0; i < entryCount; i++)
                {
                    CopiedEntryType entryType = (CopiedEntryType)memoryStream.ReadByte();

                    CopiedMapEntry entry;

                    switch (entryType)
                    {
                        case CopiedEntryType.Terrain:
                            entry = new CopiedTerrainEntry();
                            break;
                        case CopiedEntryType.Overlay:
                            entry = new CopiedOverlayEntry();
                            break;
                        case CopiedEntryType.TerrainObject:
                            entry = new CopiedTerrainObjectEntry();
                            break;
                        default:
                        case CopiedEntryType.Invalid:
                            throw new CopiedMapDataSerializationException("Invalid map data entry type " + entryType);
                    }

                    entry.ReadData(memoryStream);
                    CopiedMapEntries.Add(entry);
                }
            }
        }
    }

    /// <summary>
    /// A mutation that allows pasting terrain on the map.
    /// </summary>
    public class PasteTerrainMutation : Mutation
    {
        public PasteTerrainMutation(IMutationTarget mutationTarget, CopiedMapData copiedMapData, Point2D origin) : base(mutationTarget)
        {
            this.copiedMapData = copiedMapData;
            this.origin = origin;
        }

        private readonly CopiedMapData copiedMapData;
        private readonly Point2D origin;

        private OriginalCellTerrainData[] terrainUndoData;
        private OriginalOverlayInfo[] overlayUndoData;
        private Point2D[] terrainObjectCells;

        private void AddRefresh()
        {
            if (copiedMapData.CopiedMapEntries.Count > 10)
                MutationTarget.InvalidateMap();
            else
                MutationTarget.AddRefreshPoint(origin);
        }

        public override void Perform()
        {
            var terrainUndoData = new List<OriginalCellTerrainData>();
            foreach (var entry in copiedMapData.CopiedMapEntries.FindAll(e => e.EntryType == CopiedEntryType.Terrain))
            {
                var copiedTerrainData = entry as CopiedTerrainEntry;
                Point2D cellCoords = origin + copiedTerrainData.Offset;
                MapTile cell = MutationTarget.Map.GetTile(cellCoords);

                if (cell == null)
                    continue;

                terrainUndoData.Add(new OriginalCellTerrainData(cellCoords, cell.TileIndex, cell.SubTileIndex));

                cell.TileImage = null;
                cell.TileIndex = copiedTerrainData.TileIndex;
                cell.SubTileIndex = copiedTerrainData.SubTileIndex;
            }

            this.terrainUndoData = terrainUndoData.ToArray();

            var overlayUndoData = new List<OriginalOverlayInfo>();
            foreach (var entry in copiedMapData.CopiedMapEntries.FindAll(e => e.EntryType == CopiedEntryType.Overlay))
            {
                var copiedOverlayData = entry as CopiedOverlayEntry;
                Point2D cellCoords = origin + copiedOverlayData.Offset;
                MapTile cell = MutationTarget.Map.GetTile(cellCoords);

                if (cell == null)
                    continue;

                var overlayType = MutationTarget.Map.Rules.OverlayTypes.Find(ovt => ovt.ININame == copiedOverlayData.OverlayTypeName);
                if (overlayType == null)
                    continue;

                if (cell.Overlay == null)
                    overlayUndoData.Add(new OriginalOverlayInfo(-1, 0, cellCoords));
                else
                    overlayUndoData.Add(new OriginalOverlayInfo(cell.Overlay.OverlayType.Index, cell.Overlay.FrameIndex, cellCoords));

                cell.Overlay = new Overlay()
                {
                    FrameIndex = copiedOverlayData.FrameIndex,
                    OverlayType = overlayType,
                    Position = cellCoords
                };
            }

            this.overlayUndoData = overlayUndoData.ToArray();

            var terrainObjectCells = new List<Point2D>();
            foreach (var entry in copiedMapData.CopiedMapEntries.FindAll(e => e.EntryType == CopiedEntryType.TerrainObject))
            {
                var copiedTerrainObjectEntry = entry as CopiedTerrainObjectEntry;
                Point2D cellCoords = origin + copiedTerrainObjectEntry.Offset;
                MapTile cell = MutationTarget.Map.GetTile(cellCoords);

                if (cell == null)
                    continue;

                if (cell.TerrainObject != null)
                    continue;

                var terrainType = MutationTarget.Map.Rules.TerrainTypes.Find(tt => tt.ININame == copiedTerrainObjectEntry.TerrainObjectTypeName);
                if (terrainType == null)
                    continue;

                terrainObjectCells.Add(cellCoords);
                MutationTarget.Map.AddTerrainObject(new TerrainObject(terrainType, cellCoords));
            }
            this.terrainObjectCells = terrainObjectCells.ToArray();

            AddRefresh();
        }

        public override void Undo()
        {
            foreach (var originalTerrainData in terrainUndoData)
            {
                var cell = MutationTarget.Map.GetTile(originalTerrainData.CellCoords);
                cell.TileImage = null;
                cell.TileIndex = originalTerrainData.TileIndex;
                cell.SubTileIndex = originalTerrainData.SubTileIndex;
            }

            foreach (OriginalOverlayInfo info in overlayUndoData)
            {
                var cell = MutationTarget.Map.GetTile(info.CellCoords);
                if (info.OverlayTypeIndex == -1)
                {
                    cell.Overlay = null;
                    continue;
                }

                cell.Overlay = new Overlay()
                {
                    OverlayType = MutationTarget.Map.Rules.OverlayTypes[info.OverlayTypeIndex],
                    Position = info.CellCoords,
                    FrameIndex = info.FrameIndex
                };
            }

            foreach (Point2D cellCoords in terrainObjectCells)
            {
                MutationTarget.Map.RemoveTerrainObject(cellCoords);
            }

            AddRefresh();
        }
    }
}
