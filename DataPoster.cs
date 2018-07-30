using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net;
using System.IO;


namespace Data
{
    class DataPoster
    {
                   
        static void Main(string[] args)
        {
            // Create a request for the URL.   
            WebRequest request = WebRequest.Create(
              "https://yourun-server.herokuapp.com/locations");
            // If required by the server, set the credentials.  
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.  
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content.  
            Console.WriteLine(responseFromServer);
            // Clean up the streams and the response.  
            reader.Close();
            response.Close();

            Console.WriteLine("HELLP MEEEE");

            createPoint(1.3, 21.2, "1837");


            Console.ReadKey();
        }

        static void createPoint(double latitude, double longitude, string point_id)
        {
            WebRequest request = WebRequest.Create("https://yourun-server.herokuapp.com/locations");
            request.Method = "POST";
            string postArray = makePostRequest(latitude, longitude, point_id);
            byte[] byteArray = Encoding.UTF8.GetBytes(postArray);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);
            reader.Close();
            dataStream.Close();
            response.Close();


        }

        static string makePostRequest(double latitude, double longitude, string point_id)
        {
            //JObject jsonString = new JObject(new JProperty("point_id", point_id), new JProperty("latitude", latitude), new JProperty("longitude", longitude));
            string jsonString = "lat=" + latitude.ToString() + "&lng=" + longitude.ToString() + "&id=" + point_id.ToString();
            Console.WriteLine(jsonString);
            return jsonString;
        }
       

        
    }
}

   
 
