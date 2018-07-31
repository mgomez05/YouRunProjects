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
            createPoint(12.5, 1.4, "123455667");

            getPoints();

            Console.ReadKey();
        }

        //Posts the latitude, longitude, and point id of a point onto the online database.
        //Most of the code within this function comes from
        //https://docs.microsoft.com/en-us/dotnet/framework/network-programming/how-to-send-data-using-the-webrequest-class

        static void createPoint(double latitude, double longitude, string point_id)
        {
            // Create a request using a URL that can receive a post
            WebRequest request = WebRequest.Create("https://yourun-server.herokuapp.com/locations");
            // Set the Method property of the request to POST.
            request.Method = "POST";
            // Create POST data and convert it to a byte array.
            string postArray = makePostRequest(latitude, longitude, point_id);
            byte[] byteArray = Encoding.UTF8.GetBytes(postArray);
            // Set the ContentType property of the WebRequest. 
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest. 
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream. 
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.  
            dataStream.Close();
            // Get the response. 
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server. 
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content. 
            Console.WriteLine(responseFromServer);
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();


        }

        //Retrieves information from online database
        //Most of the code comes from
        //https://docs.microsoft.com/en-us/dotnet/framework/network-programming/how-to-request-data-using-the-webrequest-class

        static void getPoints()
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
            string jsonFromServer = reader.ReadToEnd();
            // Display the content.  
            Console.WriteLine(jsonFromServer);
            // Clean up the streams and the response.
            reader.Close();
            response.Close();
        }

        //Makes the information that would be submitted to the online database in a certain format.
        //It's the same information you would put inbetween quotation marks when using curl -d "" -X "POST
        static string makePostRequest(double latitude, double longitude, string point_id)
        {
            string jsonString = "lat=" + latitude.ToString() + "&lng=" + longitude.ToString() + "&id=" + point_id.ToString();
            Console.WriteLine("Message being sent: {0}", jsonString);
            return jsonString;
        }
       

        
    }
}

   
 
