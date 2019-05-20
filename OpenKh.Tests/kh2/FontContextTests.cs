﻿using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Contextes;
using System;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class FontContextTests
    {
        [Fact]
        public void LoadEnglishSystemFontTest() =>
            LoadFontTest(512, 256, "sys", x => x.ImageSystem);

        [Fact]
        public void LoadEnglishEventFontTest() =>
            LoadFontTest(512, 512, "evt", x => x.ImageEvent);

        [Fact]
        public void LoadIconFontTest() =>
            LoadFontTest(256, 160, "icon", x => x.ImageIcon);

        private static void LoadFontTest(
            int expectedWidth,
            int expectedHeight,
            string name,
            Func<FontContext, IImage> getter)
        {
            var expectedLength = expectedWidth * expectedHeight;

            var fontContext = new FontContext();
            var entry = new Bar.Entry
            {
                Name = name,
                Type = Bar.EntryType.RawBitmap,
                Stream = CreateStream(expectedLength)
            };

            fontContext.Read(new Bar.Entry[] { entry });

            var image = getter(fontContext);
            Assert.NotNull(image);
            Assert.Equal(expectedWidth, image.Size.Width);
            Assert.Equal(expectedHeight, image.Size.Height);
            Assert.Equal(expectedLength, entry.Stream.Position);
        }

        private static Stream CreateStream(int length)
        {
            var stream = new MemoryStream(length);
            stream.Write(new byte[length]);

            return stream;
        }
    }
}
