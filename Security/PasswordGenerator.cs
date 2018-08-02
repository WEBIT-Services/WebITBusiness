using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WebIt.Security
{
    /// <summary>
    /// Class that generates strong passwords
    /// </summary>
    public class PasswordGenerator
    {
        private const int DefaultMinimum = 6;
        private const int DefaultMaximum = 10;
        private const int UBoundDigit = 61;

        private RNGCryptoServiceProvider rng;
        private int minSize;
        private int maxSize;
        private bool hasRepeating;
        private bool hasConsecutive;
        private bool hasSymbols;
        private string exclusionSet;
        // Removed characters that are commonly confused
        private char[] pwdCharArray = "abcdefghijklmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
        private char[] pwdDigitArray = "23456789".ToCharArray();
        private char[] pwdSpecialArray = "!@#$^&*".ToCharArray();

        /// <summary>
        /// Set of characters to exclude from password
        /// </summary>
        public string Exclusions
        {
            get { return this.exclusionSet; }
            set { this.exclusionSet = value; }
        }

        /// <summary>
        /// Minimum length of the password
        /// </summary>
        public int Minimum
        {
            get { return this.minSize; }
            set
            {
                this.minSize = value;
                if (PasswordGenerator.DefaultMinimum > this.minSize)
                {
                    this.minSize = PasswordGenerator.DefaultMinimum;
                }
            }
        }

        /// <summary>
        /// Maximum length of the password
        /// </summary>
        public int Maximum
        {
            get { return this.maxSize; }
            set
            {
                this.maxSize = value;
                if (this.minSize >= this.maxSize)
                {
                    this.maxSize = PasswordGenerator.DefaultMaximum;
                }
            }
        }

        /// <summary>
        /// Flag to exclude characters in the Exclusions list
        /// </summary>
        public bool ExcludeSymbols
        {
            get { return this.hasSymbols; }
            set { this.hasSymbols = value; }
        }

        /// <summary>
        /// Flag to allow characters to repeat in the password
        /// </summary>
        public bool RepeatCharacters
        {
            get { return this.hasRepeating; }
            set { this.hasRepeating = value; }
        }

        /// <summary>
        /// Flag to allow the same character to appear next to itself in the password
        /// </summary>
        public bool ConsecutiveCharacters
        {
            get { return this.hasConsecutive; }
            set { this.hasConsecutive = value; }
        }

        /// <summary>
        /// PasswordGenerator constructor
        /// </summary>
        public PasswordGenerator()
        {
            this.Minimum = DefaultMinimum;
            this.Maximum = DefaultMaximum;
            this.ConsecutiveCharacters = false;
            this.RepeatCharacters = true;
            this.ExcludeSymbols = false;
            this.Exclusions = null;

            rng = new RNGCryptoServiceProvider();
        }
		
        /// <summary>
        /// Chooses a random number
        /// </summary>
        /// <param name="lBound"></param>
        /// <param name="uBound"></param>
        /// <returns></returns>
        protected int GetCryptographicRandomNumber(int lBound, int uBound)
        {   
            // Assumes lBound >= 0 && lBound < uBound
            // returns an int >= lBound and < uBound
            uint urndnum;   
            byte[] rndnum = new Byte[4];   
            if (lBound == uBound-1)  
            {
                // test for degenerate case where only lBound can be returned
                return lBound;
            }
                                                              
            uint xcludeRndBase = (uint.MaxValue -
                (uint.MaxValue%(uint)(uBound-lBound)));   
            
            do 
            {      
                rng.GetBytes(rndnum);      
                urndnum = System.BitConverter.ToUInt32(rndnum,0);      
            } while (urndnum >= xcludeRndBase);   
            
            return (int)(urndnum % (uBound-lBound)) + lBound;
        }

        /// <summary>
        /// Chooses a random character
        /// </summary>
        /// <returns></returns>
        protected char GetRandomCharacter()
        {            
            int upperBound = pwdCharArray.GetUpperBound(0);

            if ( true == this.ExcludeSymbols )
            {
                upperBound = PasswordGenerator.UBoundDigit;
            }

            int randomCharPosition = GetCryptographicRandomNumber(
                pwdCharArray.GetLowerBound(0), upperBound);

            char randomChar = pwdCharArray[randomCharPosition];

            return randomChar;
        }

        /// <summary>
        /// Chooses a random character
        /// </summary>
        /// <returns></returns>
        protected char GetRandomDigit()
        {
            int upperBound = pwdDigitArray.GetUpperBound(0);

            if (true == this.ExcludeSymbols)
            {
                upperBound = PasswordGenerator.UBoundDigit;
            }

            int randomCharPosition = GetCryptographicRandomNumber(
                pwdDigitArray.GetLowerBound(0), upperBound);

            char randomChar = pwdDigitArray[randomCharPosition];

            return randomChar;
        }

        /// <summary>
        /// Chooses a random character
        /// </summary>
        /// <returns></returns>
        protected char GetRandomSpecial()
        {
            int upperBound = pwdSpecialArray.GetUpperBound(0);

            if (true == this.ExcludeSymbols)
            {
                upperBound = PasswordGenerator.UBoundDigit;
            }

            int randomCharPosition = GetCryptographicRandomNumber(
                pwdSpecialArray.GetLowerBound(0), upperBound);

            char randomChar = pwdSpecialArray[randomCharPosition];

            return randomChar;
        }
        
        /// <summary>
        /// Generates a strong password based on properties set on this object
        /// </summary>
        /// <returns></returns>
        public string Generate()
        {
            // Pick random length between minimum and maximum   
            int pwdLength;
            if (this.Minimum == this.Maximum)
                pwdLength = this.Maximum;
            else
                pwdLength = GetCryptographicRandomNumber(this.Minimum, this.Maximum);

            StringBuilder pwdBuffer = new StringBuilder();
            pwdBuffer.Capacity = this.Maximum;

            // Generate random characters
            char lastCharacter, nextCharacter;

            // Initial dummy character flag
            lastCharacter = nextCharacter = '\n';

            // Add Characters to password
            for ( int i = 0; i < pwdLength - 3; i++ )
            {
                nextCharacter = GetRandomCharacter();

                if ( false == this.ConsecutiveCharacters )
                {
                    while ( lastCharacter == nextCharacter )
                    {
                        nextCharacter = GetRandomCharacter();
                    }
                }

                if ( false == this.RepeatCharacters )
                {
                    string temp = pwdBuffer.ToString();
                    int duplicateIndex = temp.IndexOf(nextCharacter);
                    while ( -1 != duplicateIndex )
                    {
                        nextCharacter = GetRandomCharacter();
                        duplicateIndex = temp.IndexOf(nextCharacter);
                    }
                }

                if ( ( null != this.Exclusions ) )
                {
                    while ( -1 != this.Exclusions.IndexOf(nextCharacter) )
                    {
                        nextCharacter = GetRandomCharacter();
                    }
                }

                pwdBuffer.Append(nextCharacter);
                lastCharacter = nextCharacter;
            }

            // Add Special characters to password
            nextCharacter = GetRandomSpecial();

            if (false == this.ConsecutiveCharacters)
            {
                while (lastCharacter == nextCharacter)
                {
                    nextCharacter = GetRandomSpecial();
                }
            }

            if (false == this.RepeatCharacters)
            {
                string temp = pwdBuffer.ToString();
                int duplicateIndex = temp.IndexOf(nextCharacter);
                while (-1 != duplicateIndex)
                {
                    nextCharacter = GetRandomSpecial();
                    duplicateIndex = temp.IndexOf(nextCharacter);
                }
            }

            if ((null != this.Exclusions))
            {
                while (-1 != this.Exclusions.IndexOf(nextCharacter))
                {
                    nextCharacter = GetRandomSpecial();
                }
            }

            pwdBuffer.Append(nextCharacter);
            lastCharacter = nextCharacter;

            // Add Digits to password
            for (int i = 0; i < 2; i++)
            {
                nextCharacter = GetRandomDigit();

                if (false == this.ConsecutiveCharacters)
                {
                    while (lastCharacter == nextCharacter)
                    {
                        nextCharacter = GetRandomDigit();
                    }
                }

                if (false == this.RepeatCharacters)
                {
                    string temp = pwdBuffer.ToString();
                    int duplicateIndex = temp.IndexOf(nextCharacter);
                    while (-1 != duplicateIndex)
                    {
                        nextCharacter = GetRandomDigit();
                        duplicateIndex = temp.IndexOf(nextCharacter);
                    }
                }

                if ((null != this.Exclusions))
                {
                    while (-1 != this.Exclusions.IndexOf(nextCharacter))
                    {
                        nextCharacter = GetRandomDigit();
                    }
                }

                pwdBuffer.Append(nextCharacter);
                lastCharacter = nextCharacter;
            }

            if ( null != pwdBuffer )
            {
                return pwdBuffer.ToString();
            }
            else
            {
                return String.Empty;
            }	
        }
    }
}