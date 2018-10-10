      Hashtable points;

            // Create new Xml Document
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();

            //xDoc.Load("/Users/shehryarmalik/Documents/youruntesting/MyFirstProject/YouRunShared/iOS/test2.xml");

            // Load the file as an xml document
            xDoc.Load(_path + "test2.xml");

            // Get elements with tag name "node" and put them in nodeTree
            System.Xml.XmlNodeList nodeTree = xDoc.GetElementsByTagName("node");

            // Check the method loaded at least 1 node
            if (nodeTree.Count == 0)
            {
                Debug.WriteLine("node Tree is NULL");
            }

            // Check if data was read in correctly
            WriteNodesAndTagsToFile(nodeTree);

            // Convert the tree to a hashtable
            points = AddTreeToHashtable(nodeTree);

            return points;