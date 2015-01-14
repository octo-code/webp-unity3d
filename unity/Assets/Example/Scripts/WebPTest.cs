
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WebP;

public class WebPTest : MonoBehaviour
{
	//------------------------------------------------------------------------------------------------------------	
    public Material m_Material = null;

    //------------------------------------------------------------------------------------------------------------	
	private void Start()
	{
        if (m_Material != null)
        {
            StartCoroutine(StartWebPExample());
        }
	}

    //------------------------------------------------------------------------------------------------------------
    private IEnumerator StartWebPExample()
    {
        var lWebStream = new WWW(@"http://cdn.octo-dev.co.uk/octo/image01.webp");

        yield return lWebStream;

    	Error lError;

        Texture2D lTexture2D = Texture2DExt.CreateTexture2DFromWebP(lWebStream.bytes, true, true, out lError);

		if (lError == Error.Success)
		{
        	m_Material.mainTexture = lTexture2D;
		}
		else
		{
			Debug.LogError("Webp Load Error : " + lError.ToString());
		}
    }
}
	
