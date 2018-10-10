using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.IO;
using System.Diagnostics;

namespace YouRunShared.iOS

{
    public class WayReader
    {
        public static void Main(string[] args)
        {
            readWays();
        }

        public static void readWays()
        {
            //Hashtable points;

            // Create new Xml Document
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();

            // Load the file as an xml document
            xDoc.Load("C:/Users/dduff/Scanner/OSMFileWithWays.xml");

            // Get elements with tag name "node" and put them in nodeTree
            System.Xml.XmlNodeList nodeTree = xDoc.GetElementsByTagName("way");

            // Check the method loaded at least 1 node
            if (nodeTree.Count == 0)
            {
                Debug.WriteLine("node Tree is NULL");
            }

            // for each way
            //  -read in important attributes (it's ID)
            //  -read in each child node reference & their attributes
            /// -READ IN TAGS AND KEY VALUE PAIRS
            //  -post it to database

            foreach (XmlNode node in nodeTree)
            {
                // Select children of "node" that have the tag "nd"
                XmlNodeList childNodes = node.SelectNodes(".//nd");

                String name = node.Attributes["id"].Value;

                Debug.WriteLine("Here are " + name + "'s children");

                foreach (XmlNode childNode in childNodes)
                {
                    Debug.WriteLine("child node " + childNode.Attributes["ref"].Value);
                }

                

                Debug.WriteLine("");
            }
        }
    }

}
