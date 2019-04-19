using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class Find : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void BtnDownload_Click(object sender, Event e)
    {
        using (WebClient wc = new WebClient())
        {
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileAsync (
                // Param1 = Link of file
                new System.Uri("http://www.sayka.com/downloads/front_view.jpg"),
                // Param2 = Path to save
                "D:\\Images\\front_view.jpg"
            );
        }
    }
    

    
    float Value; //Placeholder for progress bar val
    // Event to track the progress
    void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        Value = e.ProgressPercentage;
    }
}
