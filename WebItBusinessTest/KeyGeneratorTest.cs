using System;
using System.Collections.Generic;
using System.Text;

using WebIt.Business;
using WebIt.Business.UtilityClasses;
using WebIt.Security;

using NUnit.Framework;

namespace WebIt.Business.Test
{
    /// <summary>
    /// The KeyGeneratorTest class will contain methods to test the KeyGenerator Business Object.
    /// </summary>
    [TestFixture]
    public class KeyGeneratorTest
    {
        /// <summary>
        /// KeyGeneratorTest constructor
        /// </summary>
        public KeyGeneratorTest()
        {
        }

        /// <summary>
        /// Test the GenerateKey method
        /// </summary>
        [Test]
        public void GenerateKeyTest()
        {
            KeyGenerator kg = new KeyGenerator();
            string Key = kg.GenerateKey(EncryptionAlgorithm.Rijndael);
            Console.WriteLine(Key);
            wiString.StrToFile(Key, "E:\\appdev\\WebIt\\Key.txt");

            Assert.IsNotEmpty(Key);
        }
    }
}