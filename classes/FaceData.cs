using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classes
{
    class FaceData:FaceAttributes
    {
       public string id  = FaceData.NewID();

        private static string NewID()
        {
            Guid guid = Guid.NewGuid();
            string id = guid.ToString();
            return id;
        }
        
        public FaceData ( double age, string gender, double smile, FacialHair facialHair, EmotionScores emotion) 
        {
            this.id = NewID();
            this.Age = age;
            this.Gender = gender;
            this.Smile = smile;
            this.FacialHair = facialHair;
            this.Emotion = emotion;

        }
    }
}
