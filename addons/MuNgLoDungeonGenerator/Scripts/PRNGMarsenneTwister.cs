using System;
// ReSharper disable InconsistentNaming

/* 
   A C-program for MT19937, with initialization improved 2002/1/26.
   Coded by Takuji Nishimura and Makoto Matsumoto.

   Before using, initialize the state by using init_genrand(seed)  
   or init_by_array(init_key, key_length).

   Copyright (C) 1997 - 2002, Makoto Matsumoto and Takuji Nishimura,
   All rights reserved.                          

   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:

     1. Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.

     2. Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.

     3. The names of its contributors may not be used to endorse or promote 
        products derived from this software without specific prior written 
        permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
   A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
   NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


   Any feedback is very welcome.
   http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/emt.html
   email: m-mat @ math.sci.hiroshima-u.ac.jp (remove space)
*/


/*
 * This C# version by Craig Phillips (craig@craigtp.co.uk).  Dated: 21st October 2015.
 * Conforms to the MT19937ar specification and pass all test suites for that version.
 * Please see original URL here: http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/MT2002/emt19937ar.html
 * for original C code and test outputs.
 * This version also inherits the .NET framework's System.Random class and so can be used
 * as a "drop-in" replacement for the standard .NET Random.
 */

namespace Munglo.DungeonGenerator
{
	public class PRNGMarsenneTwister : Random
	{
		private static ulong N = 624;
		private static ulong M = 397;
		private static ulong MATRIX_A = 0x9908b0dfUL;
		private static ulong UPPER_MASK = 0x80000000UL;
		private static ulong LOWER_MASK = 0x7fffffffUL;

		private static ulong[] mt = new ulong[N];
		private static ulong mti = N + 1;

		#region Implementations of System.Random methods

		public override int Next()
		{
			// Generate the ulong value and use a modulo function to reduce it's size down to that of a signed integer.
			return Convert.ToInt32(genrand_int32() % int.MaxValue);

			// Alternative way of "mapping" an unsigned long (64 bits) to a signed int (32 bits) is to simply cast to an int
			// using the unchecked keyword, which prevents overflow exceptions and simply "loses" the MSB (most significant bits)
			// of the ulong value to generate the int.
			//return unchecked((int)genrand_int32());
		}

		public override int Next(int maxValue)
		{
			return Next(0, maxValue);
		}

		public override int Next(int minValue, int maxValue)
		{
			// Guard clause
			if (maxValue <= minValue)
			{
				throw new ArgumentException("Max value must be greater than min value.");
			}

			var randomInt = Next();
			var scale = (double)(maxValue - minValue) / (int.MaxValue - 0);
			return (int)(minValue + ((randomInt - 0) * scale));
		}

		public override void NextBytes(byte[] buffer)
		{
			for (var i = 0; i < buffer.Length; i++)
			{
				var intNum = Next(0, 255);
				buffer[i] = (byte)intNum;
			}
		}

		public override double NextDouble()
		{
			return genrand_real2();
		}

		#endregion

		public PRNGMarsenneTwister()
		{
			// Seed with random 
			// Use the built-in .NET Random class to generate the required initialization array.
			var rand = new Random();
			ulong[] init_array = new ulong[4];
			init_array[0] = Convert.ToUInt64(rand.Next());
			init_array[1] = Convert.ToUInt64(rand.Next());
			init_array[2] = Convert.ToUInt64(rand.Next());
			init_array[3] = Convert.ToUInt64(rand.Next());
		}

		public PRNGMarsenneTwister(ulong[] init)
		{
			// Seed with array of unsigned longs.
			init_by_array(init);
		}

		/* initializes mt[N] with a seed */
		public void init_genrand(ulong s)
		{
			mt[0] = s & 0xffffffffUL;
			for (mti = 1; mti < N; mti++)
			{
				mt[mti] = (1812433253UL * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
				/* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
				/* In the previous versions, MSBs of the seed affect   */
				/* only MSBs of the array mt[].                        */
				/* 2002/01/09 modified by Makoto Matsumoto             */
				mt[mti] &= 0xffffffffUL;
				/* for >32 bit machines */
			}
		}

		/* initialize by an array with array-length */
		/* init_key is the array for initializing keys */
		/* key_length is its length */
		/* slight change for C++, 2004/2/26 */
		private void init_by_array(ulong[] init_key)
		{
			int key_length = init_key.Length;

			int i, j, k;
			init_genrand(19650218UL);
			i = 1;
			j = 0;
			k = (N > (ulong)key_length ? (int)N : key_length);
			for (; k != 0; k--)
			{
				mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525UL)) + init_key[j] + (ulong)j; /* non linear */
				mt[i] &= 0xffffffffUL; /* for WORDSIZE > 32 machines */
				i++;
				j++;
				if (i >= (int)N)
				{
					mt[0] = mt[N - 1];
					i = 1;
				}
				if (j >= key_length)
					j = 0;
			}
			for (k = (int)(N - 1); k != 0; k--)
			{
				mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1566083941UL)) - (ulong)i; /* non linear */
				mt[i] &= 0xffffffffUL; /* for WORDSIZE > 32 machines */
				i++;
				if (i >= (int)N)
				{
					mt[0] = mt[N - 1];
					i = 1;
				}
			}
			mt[0] = 0x80000000UL; /* MSB is 1; assuring non-zero initial array */
		}

		public ulong genrand_int32()
		{
			ulong y;
			ulong[] mag01 = { 0x0UL, MATRIX_A };
			/* mag01[x] = x * MATRIX_A  for x=0,1 */

			if (mti >= N)
			{ /* generate N words at one time */
				int kk;

				if (mti == N + 1)   /* if init_genrand() has not been called, */
					init_genrand(5489UL); /* a default initial seed is used */

				for (kk = 0; kk < (int)(N - M); kk++)
				{
					y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
					mt[kk] = mt[kk + (int)M] ^ (y >> 1) ^ mag01[y & 0x1UL];
				}
				for (; kk < (int)(N - 1); kk++)
				{
					y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
					mt[kk] = mt[kk + (int)(M - N)] ^ (y >> 1) ^ mag01[y & 0x1UL];
				}
				y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
				mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1UL];

				mti = 0;
			}

			y = mt[mti++];

			/* Tempering */
			y ^= (y >> 11);
			y ^= (y << 7) & 0x9d2c5680UL;
			y ^= (y << 15) & 0xefc60000UL;
			y ^= (y >> 18);

			return y;
		}

		/* generates a random number on [0,0x7fffffff]-interval */
		private long genrand_int31()
		{
			return (long)(genrand_int32() >> 1);
		}

		/* generates a random number on [0,1]-real-interval */
		private double genrand_real1()
		{
			return genrand_int32() * (1.0 / 4294967295.0);
			/* divided by 2^32-1 */
		}

		/* generates a random number on [0,1]-real-interval */
		public double genrand_real2()
		{
			return genrand_int32() * (1.0 / 4294967296.0);
			/* divided by 2^32 */
		}

		/* generates a random number on [0,1]-real-interval */
		private double genrand_real3()
		{
			return (((double)genrand_int32()) + 0.5) * (1.0 / 4294967296.0);
			/* divided by 2^32 */
		}

		/* generates a random number on [0,1] with 53-bit resolution*/
		private double genrand_res53()
		{
			ulong a = genrand_int32() >> 5, b = genrand_int32() >> 6;
			return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
		}
		/* These real versions are due to Isaku Wada, 2002/01/09 added */
	}// EOF CLASS
}
