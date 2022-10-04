using System;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.Serialization;
using System.IO;

namespace NeoSaveGames
{
    public class SaveFileMetaData : INeoSerializableObject
    {
        private static readonly NeoSerializationKey k_SaveTypeKey = new NeoSerializationKey("saveType");
        private static readonly NeoSerializationKey k_TitleKey = new NeoSerializationKey("title");
        private static readonly NeoSerializationKey k_SaveTimeKey = new NeoSerializationKey("saveTime");
        private static readonly NeoSerializationKey k_HasThumbnailKey = new NeoSerializationKey("hasThumbnail");
        private static readonly NeoSerializationKey k_ThumbnailSizeKey = new NeoSerializationKey("thumbnailSize");
        private static readonly NeoSerializationKey k_ThumbnailFormatKey = new NeoSerializationKey("thumbnailFormat");
        private static readonly NeoSerializationKey k_ThumbnailDataKey = new NeoSerializationKey("thumbnailData");

        private string m_Title = string.Empty;
        private int m_SaveType = (int)SaveGameType.Quicksave;
        private Texture2D m_Thumbnail = null;
        private DateTime m_SaveTime = new DateTime();
        private FileInfo m_SaveFile = null;

        public string title { get { return m_Title; } }
        public SaveGameType saveType { get { return (SaveGameType)m_SaveType; } }
        public Texture2D thumbnail { get { return m_Thumbnail; } }
        public DateTime saveTime { get { return m_SaveTime; } }
        public FileInfo saveFile { get { return m_SaveFile; } }

        public SerializationContext serializableContext
        {
            get { return SerializationContext.MetaData; }
        }

        public bool loaded
        {
            get;
            private set;
        }

        public SaveFileMetaData(FileInfo saveFile)
        {
            m_SaveFile = saveFile;
            loaded = false;
        }

        public SaveFileMetaData(string title, SaveGameType saveType, DateTime saveTime, Texture2D thumbnail)
        {
            m_Title = title;
            m_SaveType = (int)saveType;
            m_SaveTime = saveTime;
            m_Thumbnail = thumbnail;
            loaded = true;

            // FileInfo???
        }

        public virtual void WriteProperties(INeoSerializer writer)
        {
            writer.WriteValue(k_SaveTypeKey, m_SaveType);
            writer.WriteValue(k_TitleKey, m_Title);
            writer.WriteValue(k_SaveTimeKey, m_SaveTime);

            if (m_Thumbnail != null)
            {
                writer.WriteValue(k_HasThumbnailKey, true);
                writer.WriteValue(k_ThumbnailSizeKey, new Vector2Int(m_Thumbnail.width, m_Thumbnail.height));
                writer.WriteValue(k_ThumbnailFormatKey, (int)m_Thumbnail.format);
                writer.WriteValues(k_ThumbnailDataKey, m_Thumbnail.GetRawTextureData());
            }
            else
                writer.WriteValue(k_HasThumbnailKey, false);
        }

        public virtual void ReadProperties(INeoDeserializer reader)
        {
            reader.TryReadValue(k_SaveTypeKey, out m_SaveType, m_SaveType);
            reader.TryReadValue(k_TitleKey, out m_Title, m_Title);
            reader.TryReadValue(k_SaveTimeKey, out m_SaveTime, m_SaveTime);

            try
            {
                bool hasThumbnail;
                reader.TryReadValue(k_HasThumbnailKey, out hasThumbnail, false);
                if (hasThumbnail)
                {
                    byte[] thumbnailData;
                    if (reader.TryReadValues(k_ThumbnailDataKey, out thumbnailData, null) && thumbnailData != null)
                    {
                        Vector2Int size;
                        reader.TryReadValue(k_ThumbnailSizeKey, out size, Vector2Int.zero);
                        int format;
                        reader.TryReadValue(k_ThumbnailFormatKey, out format, (int)TextureFormat.ARGB32);
                        m_Thumbnail = new Texture2D(size.x, size.y, (TextureFormat)format, false);
                        m_Thumbnail.LoadRawTextureData(thumbnailData);
                        m_Thumbnail.filterMode = FilterMode.Bilinear;
                        m_Thumbnail.Apply();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load save metadata due to error: " + e.Message);
            }

            loaded = true;
        }
    }
}