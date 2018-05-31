using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.ProjectOxford.Face;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classes
{
    class Program
    {
        static void Main(string[] args)
        {

            IFaceServiceClient faceServiceClient = new FaceServiceClient("key", "https://northeurope.api.cognitive.microsoft.com/face/v1.0");
            string personGroupId = "groupid";
            string path = @"C:\Users\avalo\source\repos\opencv3\opencv3\bin\Debug\TEMP\2017-11-16 (6).png";

            //Database configure 

            // The endpoint to your cosmosdb instance
            var endpointUrl = "https://xxx.documents.azure.com:443/";

            // The key to you cosmosdb
            var key = "key";

            // The name of the database
            var databaseName = "ConSystem";

            // The name of the collection of json documents
            var databaseCollection = "FacesCollection";

            using (var client = new DocumentClient(new Uri(endpointUrl), key))
            {
                // Create the database
                client.CreateDatabaseIfNotExistsAsync(new Database() { Id = databaseName }).GetAwaiter().GetResult();

                // Create the collection
                client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(databaseName),
                    new DocumentCollection { Id = databaseCollection }).
                    GetAwaiter()
                    .GetResult();
            }
            Task.Run(async () =>
            {
                using (Stream s = File.OpenRead(path)) //get file with faces
                {
                    //check empty file or not
                    if (s.Length > 0)
                    {
                        var requiredFaceAttributes = new FaceAttributeType[] //выбираю только нужные аттрибуты
                        {
                          FaceAttributeType.Age,
                          FaceAttributeType.Gender,
                          FaceAttributeType.Smile,
                          FaceAttributeType.FacialHair,
                          FaceAttributeType.Glasses,
                          FaceAttributeType.Emotion
                        };
                        //засылаю фотку в когнитивные сервисы 
                        var faces = await faceServiceClient.DetectAsync(s, true, false, returnFaceAttributes: requiredFaceAttributes);//detect faces
                        var faceIds = faces.Select(face => face.FaceId).ToArray(); // get FaceID for Identify method
                        //db client init
                        var client = new DocumentClient(new Uri(endpointUrl), key);

                        var facedates = faces.Select(face => new FaceData
                        (
                            
                            face.FaceAttributes.Age,
                            face.FaceAttributes.Gender,
                            face.FaceAttributes.Smile,
                            face.FaceAttributes.FacialHair,
                            face.FaceAttributes.Emotion
                        )
                           );

                        foreach (var fd in facedates)
                        {
                            Console.WriteLine(fd.id);
                            Console.WriteLine(fd.Age);
                            Console.WriteLine(fd.Gender);
                            Console.WriteLine(fd.Smile);
                            Console.WriteLine(fd.FacialHair);
                            Console.WriteLine(fd.Emotion);

                            // Write data to DB foreach face
                            client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, databaseCollection), fd)
                       .GetAwaiter().GetResult();
                        }

                        if (faceIds.Length > 0)
                        {
                            var results = await faceServiceClient.IdentifyAsync(faceIds, personGroupId); //get PersonIDs
                            foreach (var identifyResult in results)
                            {
                                Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                                if (identifyResult.Candidates.Length == 0)
                                {
                                    Console.WriteLine("No one identified");
                                }
                                else
                                {
                                    // Get top 1 among all candidates returned
                                    var candidateId = identifyResult.Candidates[0].PersonId;
                                    var person = await faceServiceClient.GetPersonInPersonGroupAsync(personGroupId, candidateId);// get Person name
                                    Console.WriteLine("Identified as {0}", person.Name);

                                    


                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("{0} faces founded");
                           
                        }
                    }
                }
                

            }).GetAwaiter().GetResult();
            Console.ReadKey();
        }
        
    }
}
