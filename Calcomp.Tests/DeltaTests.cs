using System;

using NUnit.Framework;

using Calcomp;

namespace Calcomp.Tests {
    [TestFixture]
    public class DeltaTests {
        /// <summary>
        /// Test the constructor with command byte values above and below the acceptable range
        /// </summary>
        [Test]
        public void Delta_invalidByteLow_Constructor() {
            Assert.That(() => new Delta(0x9, 95), Throws.ArgumentException);
        }

        [Test]
        public void Delta_invalidByteHigh_Constructor() {
            Assert.That(() => new Delta(0x40, 95), Throws.ArgumentException);
        }

        /// <summary>
        /// Test the expected byte count values for a range of command byte values. There are
        /// 48 different values but we only need to test 12 (stepping up 4 each time) to ensure 
        /// we're calculating the expected bytes properly (the remainder is used to calculate the
        /// signs of the deltas, which is handled in the detailed tests later on)
        /// </summary>
        [Test]
        public void Delta_instructionByte0x10_ByteCount() {
            Delta delta = new Delta(0x10, 95);

            Assert.That(delta.ExpectedByteCount,Is.EqualTo(6));
        }

        [Test]
        public void Delta_instructionByte0x14_ByteCount() {
            Delta delta = new Delta(0x14, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(4));
        }

        [Test]
        public void Delta_instructionByte0x18_ByteCount() {
            Delta delta = new Delta(0x18, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(2));
        }

        [Test]
        public void Delta_instructionByte0x1c_ByteCount() {
            Delta delta = new Delta(0x1c, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(3));
        }

        [Test]
        public void Delta_instructionByte0x20_ByteCount() {
            Delta delta = new Delta(0x20, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(2));
        }

        [Test]
        public void Delta_instructionByte0x24_ByteCount() {
            Delta delta = new Delta(0x24, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(1));
        }

        [Test]
        public void Delta_instructionByte0x28_ByteCount() {
            Delta delta = new Delta(0x28, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(5));
        }

        [Test]
        public void Delta_instructionByte0x2c_ByteCount() {
            Delta delta = new Delta(0x2c, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(4));
        }

        [Test]
        public void Delta_instructionByte0x30_ByteCount() {
            Delta delta = new Delta(0x30, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(5));
        }

        [Test]
        public void Delta_instructionByte0x34_ByteCount() {
            Delta delta = new Delta(0x34, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(4));
        }

        [Test]
        public void Delta_instructionByte0x38_ByteCount() {
            Delta delta = new Delta(0x38, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(3));
        }

        [Test]
        public void Delta_instructionByte0x3c_ByteCount() {
            Delta delta = new Delta(0x3c, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(3));
        }

        /// <summary>
        /// Test the logic enforcing the correct number of bytes
        /// </summary>
        [Test]
        public void Delta_tooManyBytes_AddByte() {
            // 0x18 expects 2 bytes
            Delta delta = new Delta(0x18, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(2));

            // add 3 bytes - last call should throw InvalidOperationException
            delta.AddByte(0x10);
            delta.AddByte(0x20);

            Assert.That(() => delta.AddByte(0x30), Throws.InvalidOperationException);
        }

        [Test]
        public void Delta_tooFewBytes_AddByte() {
            // 0x18 expects 2 bytes
            Delta delta = new Delta(0x18, 95);

            Assert.That(delta.ExpectedByteCount, Is.EqualTo(2));

            // add 1 byte - call to Dx should throw InvalidOperationException
            delta.AddByte(0x10);
            
            Assert.That(() => delta.Dx, Throws.InvalidOperationException);
        }
        
        // Test specific deltas - add the relevant bytes and check for the right Dx/Dy
        // test: 4 sign combinations, 0 length x, 0 length y, 3 digits for both
    }
}
