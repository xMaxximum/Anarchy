﻿using System;

namespace Discord
{
    public class DiscordAttachmentFile
    {
        public DiscordAttachmentFile(byte[] bytes, string mediaType = null)
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentException("May not be null and Length must be > 0.", nameof(bytes));

            Bytes = bytes;
            MediaType = mediaType;
        }

        public byte[] Bytes { get; }
        public string MediaType { get; }
    }
}
