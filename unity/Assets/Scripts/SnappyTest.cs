
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Snappy;

public class SnappyTest : MonoBehaviour
{
	//------------------------------------------------------------------------------------------------------------	
    public GUIText m_Text = null;

	#region TEST_DATA
	
	private const string m_TestData =  
		"This is the February 1992 Project Gutenberg release of:\n" +
		"\n" + 
		"Paradise Lost by John Milton\n" + 
		"\n" +
		"The oldest etext known to Project Gutenberg (ca. 1964-1965)\n" + 
		"(If you know of any older ones, please let us know.)\n" +  
		"\n" + 
		"\n" + 
		"Introduction  (one page)\n" +  
		"\n" +  
		"This etext was originally created in 1964-1965 according to Dr.\n" +  
		"Joseph Raben of Queens College, NY, to whom it is attributed by\n" +  
		"Project Gutenberg.  We had heard of this etext for years but it\n" +  
		"was not until 1991 that we actually managed to track it down to\n" +  
		"a specific location, and then it took months to convince people\n" +  
		"to let us have a copy, then more months for them actually to do\n" +  
		"the copying and get it to us.  Then another month to convert to\n" +  
		"something we could massage with our favorite 486 in DOS.  After\n" +  
		"that is was only a matter of days to get it into this shape you \n" + 
		"will see below.  The original was, of course, in CAPS only, and\n" +  
		"so were all the other etexts of the 60's and early 70's.  Don't\n" +  
		"let anyone fool you into thinking any etext with both upper and\n" +  
		"lower case is an original; all those original Project Gutenberg\n" +  
		"etexts were also in upper case and were translated or rewritten\n" +  
		"many times to get them into their current condition.  They have\n" +  
		"been worked on by many people throughout the world.\n" +  
		"\n" +  
		"In the course of our searches for Professor Raben and his etext\n" +  
		"we were never able to determine where copies were or which of a\n" +  
		"variety of editions he may have used as a source.  We did get a\n" +  
		"little information here and there, but even after we received a\n" +  
		"copy of the etext we were unwilling to release it without first\n" +  
		"determining that it was in fact Public Domain and finding Raben\n" +  
		"to verify this and get his permission.  Interested enough, in a\n" +  
		"totally unrelated action to our searches for him, the professor\n" +  
		"subscribed to the Project Gutenberg listserver and we happened,\n" +  
		"by accident, to notice his name. (We don't really look at every\n" +  
		"subscription request as the computers usually handle them.) The\n" +  
		"etext was then properly identified, copyright analyzed, and the\n" +  
		"current edition prepared.\n" + 
		"\n" + 
		"To give you an estimation of the difference in the original and\n" + 
		"what we have today:  the original was probably entered on cards\n" +  
		"commonly known at the time as 'IBM cards' (Do Not Fold, Spindle\n" +  
		"or Mutilate) and probably took in excess of 100,000 of them.  A\n" +  
		"single card could hold 80 characters (hence 80 characters is an\n" +  
		"accepted standard for so many computer margins), and the entire\n" + 
		"original edition we received in all caps was over 800,000 chars\n" +  
		"in length, including line enumeration, symbols for caps and the\n" +  
		"punctuation marks, etc., since they were not available keyboard\n" +  
		"characters at the time (probably the keyboards operated at baud\n" +  
		"rates of around 113, meaning the typists had to type slowly for\n" + 
		"the keyboard to keep up).\n" +  
		"\n" +  
		"This is the second version of Paradise Lost released by Project\n" +  
		"Gutenberg.  The first was released as our October, 1991 etext.\n";

	#endregion
	
    //------------------------------------------------------------------------------------------------------------	
	private void Start()
	{
		if (m_Text != null)
		{
			string lCompressString = CompressString(m_TestData);
			string lDecompressString = DecompressString(lCompressString);
		
			m_Text.text = 
				"Compressed String : \n" + lCompressString + 
				"Decompressed String : \n" + lDecompressString;
		}
	}
	
    //------------------------------------------------------------------------------------------------------------
    public static string CompressString(string lEncode)
    {
        byte[] lEncodeBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(lEncode);
        byte[] lCompressedBytes = SnappyCodec.Compress(lEncodeBytes); 
        return System.Convert.ToBase64String(lCompressedBytes);
    }
	
    //------------------------------------------------------------------------------------------------------------
    public static string DecompressString(string lEncodedData)
    {
        byte[] lEncodedBytes = System.Convert.FromBase64String(lEncodedData);
        byte[] lDecompressedBytes = SnappyCodec.Uncompress(lEncodedBytes);
        string lDecodedData = System.Text.ASCIIEncoding.ASCII.GetString(lDecompressedBytes);
        return lDecodedData;
    }
}
