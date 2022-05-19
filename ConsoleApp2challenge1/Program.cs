using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Text;
using ScreenshotCaptureWithMouse.ScreenCapture;
//purpose
//this program is guessing (deciding) the best thing to do based on patterns it has found in what it has discovered
/*in human all decisions are made while we are awake, and they are 2 types:
 *  1-we receive reality we like it comparing to our situation and the pleasure we have experienced,it remind us of so many behaviors we chose the best when we can or just while using this neuron over and over again until finding a way to calibrate between the previouse good exprience and the current situation.
 *  2-we receive reality we loathe it comparing to our situation and the pleasure we have experienced, we receive pleasure from a better thing in mind while using this neuron over and over again until finding a way to calibrate between the previouse good exprience and the current situation.
 * all decisions are concequences of:
 *  1.receiving from receptor.
 *  2.comparing from memory the alike.
 *if this alike doesn't exist it needs to be created, if it is too much to be created or to analize can cause pain while familiar things are less painful.
 *we choose the readiest we choose the easiest we choose the pleasurable by experience.
 */

//there is no attraction for good or bad when we visit memory if we have enough energy we cann see its concequences including if it is good or bad and it is us who decide if we want to go for it even its bad and we know it bad but always for some good reason . so as in memory or reality we can go for painful things if we want.
namespace ConsoleApp2challenge1
{
    class Program
    {

        #region input image

        static String  path;
        static Bitmap  capfile;
        public static void extractGrayScales(Bitmap bmap)
        {

            unsafe
            {
                //based on a claculation based on the article in https://www.arabicprogrammer.com/article/8771957274/  it takes 62 lockbites to make set pixel the same speed as lockbite
                BitmapData bitmapData = bmap.LockBits(new Rectangle(0, 0, bmap.Width, bmap.Height), ImageLockMode.ReadWrite, bmap.PixelFormat);//choosing a section to be changed in memrory

                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(bmap.PixelFormat) / 8;//8 to go bits from bytes //getting the size of the pixel, will help when moving the next color division
                int heightInPixels = bitmapData.Height;//a variable to prevent calculating height eachtime,N°bytes=N°pixels
                int widthInBytes = bitmapData.Width * bytesPerPixel;//a variable to prevent calculating height eachtime, in bytes
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;//neglects anything other than data and gets positionned in the first byte
                int[,] imgarr = new int[widthInBytes / 3, heightInPixels];//there are three bytes in the color division

                Parallel.For(0/*starting position*/, heightInPixels/*for ending when exceeded*/, y =>
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride/*The stride  is the number of bytes that the bitmap takes to move a memory pointer down a row*/);//it moves from the first position to the current position while taking into concideration the row number while negelecting thes stride

                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int avg = ((currentLine[x] + currentLine[x + 1] + currentLine[x + 2]) / 3);



                        //   currentLine[x] = (byte)avg;//these 3 lines will make the image a gray scale
                        //   currentLine[x + 1] = (byte)avg;
                        //   currentLine[x + 2] = (byte)avg;

                        imgarr[x, y] = avg;
                    }

                }

                );

                describer(imgarr);
                bmap.UnlockBits(bitmapData);// prevents errors like memory leak(adress modifiction) or access violation (unavailable adresses modification).

            }

            System.GC.Collect();// disposing the bitmap isn't enough to get rid of its garbage we need to garbage collection
            System.GC.WaitForPendingFinalizers();//Suspends the current thread until the thread that is processing the GC of the queue has emptied that queue.
            

        }

      
        public static List<List<List<List<List<int>>>>> storyOfPixelslist = new List<List<List<List<List<int>>>>>(); 
        //1shape(2onelinedraw(3pixels(4position(5x-->most needed diffrentiation),y),shape-->first partion contains the value other remain unused,story,color,N connections,connectionsposition(5x,y))))-->5,
        //[shape1][onelinedraw1][pixel1][0positionx/1positiony/2color/3connectionnumber/4connectionpositionx/5connectionpositionx/6direction/7branchingpixel][values...->]
        //it is used in the abreviator and describer so it must be mentioned globally

       public static List<List<List<List<List<int>>>>> directionlist = new List<List<List<List<List<int>>>>>();
        //to do the problem is that I should mention from which pixel this line is
        //[shape][onelinedraw][pixel][0positionofthefirstpixel/1color/2orientation/3repetivity][values].
        static int onelinedraw = -1, shapenumber = -1, pixelnumber = -1, abreviatedpixelnumber = -1;//these variables are declared globally because they used in two lists
        private static void describer(int[,] imgarr)
        {
            int height = imgarr.GetLength(0);
            int width = imgarr.GetLength(1);

            int the_checked_position_value = -1;//used in diffrent loops where it's initialized

            //todo  make the edges positive and the inner negative in the edgeslist
            int[,] edgeslist = new int[width, height];
            int ll = -1, rr = -1, uu = -1, dd = -1, ld = -1, lu = -1, rd = -1, ru = -1;
            int nextx = -1, nexty = -1, branchingpixel = -1;
            int connectionsNumber = 0;
            List<List<List<int>>> connection_n_scan = new List<List<List<int>>>();//a list to store the unused connections
            //[pixel1][y/x/direction][values...->] the pixel number is unique on every image

            int numberofuncheckededges = 0;//the while variable

            //edges detection loop//checked 1 time , and it's correct

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    the_checked_position_value = imgarr[x, y];
                    //it must not be out of borderes left(ll) right up down 
                    if ((x - 1) > 0 && (y + 1) > height) { ld = imgarr[x - 1, y + 1]; }
                    if ((x - 1) > 0) { ll = imgarr[x - 1, y]; }
                    if ((y + 1) < height) { dd = imgarr[x, y + 1]; }
                    if ((x + 1) < width && (y + 1) > height) { rd = imgarr[x + 1, y + 1]; }
                    if ((x + 1) < width) { rr = imgarr[x + 1, y]; }
                    if ((x + 1) < width && (y - 1) < 0) { ru = imgarr[x + 1, y - 1]; }
                    if ((y - 1) > 0) { uu = imgarr[x, y - 1]; }
                    if ((x - 1) > 0 && (y - 1) < 0) { lu = imgarr[x - 1, y - 1]; }



                    //we check if positive because by default it's negative and the variable are different the checked position

                    if ((ll > 0 && ll != the_checked_position_value /*if it is in contact with the sournding it is an edge, if at least on of the sourrning isnot itself it's an edge;it should be surrounded by it self*/) || (rr > 0 && rr != the_checked_position_value) || (uu > 0 && uu != the_checked_position_value) || (dd > 0 && dd != the_checked_position_value) || (ld > 0 && ld != the_checked_position_value) || (lu > 0 && lu != the_checked_position_value) || (rd > 0 && rd != the_checked_position_value) || (ru > 0 && ru != the_checked_position_value))
                    {
                        edgeslist[x, y] = the_checked_position_value;//it is an edge
                        numberofuncheckededges++;


                    }
                    else { edgeslist[x, y] = -1; }


                }

            }

            int ye = 0, xe = 0;//declared globally because the coorect value will be maintained throught the while
            while (numberofuncheckededges != 0)
            {
                if (nextx < 0)//when no connections(in the begginning and a new shape) left in that same color the nextx becomes<0 else nextx should be filled ////positionning if //checked 1 time , and it's correct
                {
                    for (; ye < height; ye++)
                    {
                        for (; xe < width; xe++)
                        {  //"fors" bellow checking that this position is never used before.
                            for (int i = 0; i < shapenumber; i++)
                            {
                                for (int j = 0; j < onelinedraw; j++)
                                {
                                    for (int k = 0; k < pixelnumber; k++)
                                    {
                                        if (xe != storyOfPixelslist[i][j][k][0][0] && ye != storyOfPixelslist[i][j][k][1][0])
                                        {
                                            the_checked_position_value = edgeslist[xe, ye];//after reaching an unchecked edge we go to that place.
                                            goto ThatPlace;
                                        }
                                    }
                                }
                            }
                        }
                    } //atributes to diffrenciate over quantity, a percentage of accurence taking the longer one with comparison to the smaller one
                ThatPlace:;
                }
                else
                {
                    xe = nextx;
                    ye = nexty;
                }

                connectionsNumber = 0;//preparing it for the next pixel

                if (the_checked_position_value > 0)/*if it is an edge*/
                {
                    numberofuncheckededges--;
                    if (shapenumber < 0)//storyOfPixelslist initiator if it should happen only once in this holding while .
                    {
                        /*if nothing is used yet*/
                        shapenumber++; onelinedraw++;
                        storyOfPixelslist.Add(new List<List<List<List<int>>>>());//shape must increase before we start
                        storyOfPixelslist[shapenumber].Add(new List<List<List<int>>>());//one line drawing must increase before we start
                    }//for the first time we need it to be 0 for the next we will increment shape number when there is no connection left and we eincrement onelined drow when there is no connections next

                    pixelnumber++;
                    storyOfPixelslist[shapenumber][onelinedraw].Add(new List<List<int>>());//pixel is used at the initial use and everytime that'w why it is outside the initiator if//for the story line we can copy the metual part


                    #region//this region is for initialising lists and for giving the values that doesn't need further calculation

                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber].Add(new List<int>());//positionx0
                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber].Add(new List<int>());//positiony1
                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][0].Add(xe);
                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][1].Add(ye);
                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber].Add(new List<int>());//color2
                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][2].Add(the_checked_position_value);
                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber].Add(new List<int>());//numberofconnections3
                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber].Add(new List<int>());//connectionpositionx4
                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber].Add(new List<int>());//connectionpositiony5
                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber].Add(new List<int>());//direction6                                                     
                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber].Add(new List<int>());//branchingfromwhat or not7                                                      

                    connection_n_scan.Add(new List<List<int>>());//initialising it for use.
                    #endregion

                    #region//this region is for connections where we count the number of connections we fill it in (the list of the unsued connections and in the story list)

                    //depending on what's acheivable we make the number of connection increase when found we add the found connections and we make it in the order of what's useful for the story
                    if ((xe - 1) > 0 && (ye + 1) > height)
                    {
                        if (ld == the_checked_position_value)//0
                        {
                            connectionsNumber++;
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][4].Add(xe);//connectionpositionx
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][5].Add(ye);//connectionpositiony
                            connection_n_scan[pixelnumber][0].Add(xe);
                            connection_n_scan[pixelnumber][1].Add(ye);
                            connection_n_scan[pixelnumber][2].Add(0);
                        }
                    }
                    if ((xe - 1) > 0)
                    {
                        if (ll == the_checked_position_value) //1
                        {
                            connectionsNumber++;
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][4].Add(xe);//connectionpositionx
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][5].Add(ye);//connectionpositiony
                            connection_n_scan[pixelnumber][0].Add(xe);
                            connection_n_scan[pixelnumber][1].Add(ye);
                            connection_n_scan[pixelnumber][2].Add(1);

                        }
                    }
                    if ((ye + 1) < height)
                    {
                        if (dd == the_checked_position_value)//2
                        {
                            connectionsNumber++;
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][4].Add(xe);//connectionpositionx
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][5].Add(ye);//connectionpositiony
                            connection_n_scan[pixelnumber][0].Add(xe);
                            connection_n_scan[pixelnumber][1].Add(ye);
                            connection_n_scan[pixelnumber][2].Add(2);

                        }
                    }
                    if ((xe + 1) < width && (ye + 1) > height)
                    {
                        if (rd == the_checked_position_value)//3
                        {
                            connectionsNumber++;

                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][4].Add(xe);//connectionpositionx
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][5].Add(ye);//connectionpositiony
                            connection_n_scan[pixelnumber][0].Add(xe);
                            connection_n_scan[pixelnumber][1].Add(ye);
                            connection_n_scan[pixelnumber][2].Add(3);


                        }
                    }
                    if ((xe + 1) < width)
                    {
                        if (rr == the_checked_position_value)//4
                        {
                            connectionsNumber++;
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][4].Add(xe);//connectionpositionx
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][5].Add(ye);//connectionpositiony
                            connection_n_scan[pixelnumber][0].Add(xe);
                            connection_n_scan[pixelnumber][1].Add(ye);
                            connection_n_scan[pixelnumber][2].Add(4);

                        }
                    }
                    if ((xe + 1) < width && (ye - 1) < 0)
                    {
                        if (ru == the_checked_position_value)//5
                        {
                            connectionsNumber++;
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][4].Add(xe);//connectionpositionx
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][5].Add(ye);//connectionpositiony
                            connection_n_scan[pixelnumber][0].Add(xe);
                            connection_n_scan[pixelnumber][1].Add(ye);
                            connection_n_scan[pixelnumber][2].Add(5);

                        }
                    }
                    if ((ye - 1) > 0)
                    {
                        if (uu == the_checked_position_value)//6
                        {
                            connectionsNumber++;
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][4].Add(xe);//connectionpositionx
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][5].Add(ye);//connectionpositiony
                            connection_n_scan[pixelnumber][0].Add(xe);
                            connection_n_scan[pixelnumber][1].Add(ye);
                            connection_n_scan[pixelnumber][2].Add(6);

                        }
                    }
                    if ((xe - 1) > 0 && (ye - 1) < 0)
                    {
                        if (lu == the_checked_position_value)//7
                        {
                            connectionsNumber++;

                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][4].Add(xe);//connectionpositionx
                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][5].Add(ye);//connectionpositiony
                            connection_n_scan[pixelnumber][0].Add(xe);
                            connection_n_scan[pixelnumber][1].Add(ye);
                            connection_n_scan[pixelnumber][2].Add(7);
                        }
                    }


                    storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][3].Add(connectionsNumber);
                    #endregion

                    #region//spreading descision 
                    if (connectionsNumber == 0)
                    {
                        onelinedraw++;//in both next cases where we have new shape or new line we shouldstart be making the line draw so it must be mentioned here to not repeate it in if
                        storyOfPixelslist[shapenumber].Add(new List<List<List<int>>>());
                        //if we reached a pixel that doesn't have a connection and there is no connections left from before this means the entire shape is analized else we are still in the same shape just diffrent line of it
                        if (connection_n_scan[0].Count == 0)
                        {

                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][7].Add(branchingpixel);//the current branchingpixel  should be stored before moving to the next shape
                            // this shape is dead no need to remain the branching pixel would be
                            branchingpixel = -1;

                            //if there are no stored connections we follow the discovery  direction to get to the next shape
                            nextx = -1;//this is enough to make it loop for the next shape
                            storyOfPixelslist.Add(new List<List<List<List<int>>>>());

                        }
                        else
                        {
                            //we increase the story line by 1 which is already done, we move to the first stored connection.
                            branchingpixel = pixelnumber;
                            nextx = connection_n_scan[pixelnumber][0][0];
                            nexty = connection_n_scan[pixelnumber][1][0];

                            storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][6].Add(connection_n_scan[pixelnumber][2][0]);//direction                                                                                                                                  //we should always, remove the used connction
                            connection_n_scan[pixelnumber][0].RemoveAt(0);
                            connection_n_scan[pixelnumber][1].RemoveAt(0);


                        }
                    }
                    else
                    {
                        storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][7].Add(branchingpixel);//first line has to go through this
                        //if connections exist in a pixel move to the first connection  it means we are still in the same line and shape
                        nextx = connection_n_scan[pixelnumber][0][0];
                        nexty = connection_n_scan[pixelnumber][1][0];
                        storyOfPixelslist[shapenumber][onelinedraw][pixelnumber][6].Add(connection_n_scan[pixelnumber][2][0]);//direction
                                                                                                                              //we should always, remove the used connction
                        connection_n_scan[pixelnumber][0].RemoveAt(0);
                        connection_n_scan[pixelnumber][1].RemoveAt(0);
                    }
                    #endregion
                }//if positive        
                 //newly found connections will have preference in the direction we are heading to so we must add to the awaiting positions in an order that makes the previous values less important --example normal add-->0,1,2 --example insert at 0-->2,1,0, and don't forget to check if this position is used before or not, flip the order of connections
            }
        }
        //while the describer describes the shape, the abreviator makes it focused 
        private void abreviator()
        {


            //for the comming list every repeated number of orientation shall be replaced with a star
            int rootpixel;
            for (int i = 0; i < shapenumber; i++)
            {
                directionlist.Add(new List<List<List<List<int>>>>());
                for (int j = 0; j < onelinedraw; j++)
                {
                    directionlist[shapenumber].Add(new List<List<List<int>>>());
                    for (int k = 0; k < pixelnumber; k++)
                    {

                        if (abreviatedpixelnumber == -1)
                        {
                            abreviatedpixelnumber++;//we start by making the initial nessecities for directionlist
                            directionlist[i][j].Add(new List<List<int>>()); //0positionofThefirstpixel

                            directionlist[i][j][abreviatedpixelnumber].Add(new List<int>()); //0positionofThefirstpixel
                            rootpixel = pixelnumber;
                            directionlist[shapenumber][onelinedraw][abreviatedpixelnumber][0].Add(rootpixel);

                            directionlist[i][j][abreviatedpixelnumber].Add(new List<int>()); //1color
                            directionlist[i][j][abreviatedpixelnumber][1].Add(storyOfPixelslist[i][j][k][2][0]);
                            directionlist[i][j][abreviatedpixelnumber].Add(new List<int>()); //2orientation 
                            directionlist[i][j][abreviatedpixelnumber][2].Add(storyOfPixelslist[i][j][k][6][0]);
                            directionlist[i][j][abreviatedpixelnumber].Add(new List<int>()); //3repetivity
                            directionlist[i][j][abreviatedpixelnumber][3].Add(0);

                        }
                        else
                        {//if the story has the same direction as the previouse orientation no need to add new abreviatedpixelnumber

                            if (storyOfPixelslist[i][j][k][6][0] == directionlist[i][j][abreviatedpixelnumber - 1][2][0])
                            {
                                directionlist[i][j][abreviatedpixelnumber][3][0]++;
                            }
                            else
                            {
                                abreviatedpixelnumber++;
                                directionlist[i][j].Add(new List<List<int>>()); //0positionofThefirstpixel
                                directionlist[i][j][abreviatedpixelnumber].Add(new List<int>()); //0positionofThefirstpixel
                                rootpixel = pixelnumber;
                                directionlist[shapenumber][onelinedraw][abreviatedpixelnumber][0].Add(rootpixel);
                                directionlist[i][j][abreviatedpixelnumber].Add(new List<int>()); //1color
                                directionlist[i][j][abreviatedpixelnumber][1].Add(storyOfPixelslist[i][j][k][2][0]);
                                directionlist[i][j][abreviatedpixelnumber].Add(new List<int>()); //2orientation 
                                directionlist[i][j][abreviatedpixelnumber][2].Add(storyOfPixelslist[i][j][k][6][0]);
                                directionlist[i][j][abreviatedpixelnumber].Add(new List<int>()); //3repetivity
                                directionlist[i][j][abreviatedpixelnumber][3].Add(0);
                            }
                        }

                    }
                }
            }

        }
        #endregion
        static List<Thing> List1 = new List<Thing>();//saving into file
        static List<Thing> ListOfGetting = new List<Thing>();//getting from the file
        static List<Thing> foundInEYE = new List<Thing>();//
        static List<Thing> Focus_area = new List<Thing>();
        static List<Thing> DecisionOrder = new List<Thing>();
        static int SystemEnergy = 100;//this variable is important else the program will be running forever take in all your precious ressources

        /*////consciousness/////
         * 1.input 
         * 2.T1 analysis:extracting objects if there is any from the "Object abbreviating order" based on how close to the feature and its locations
         * 2.T2 analysing the frame and and following story if there is any: by diffrence  of features (intensities, soble,directions) for objects, and by objects for stories
         * 3.T3 competing inerest with a comparison to the order of the object in the list of "Absolute value deciding order" 
         *  go for the most pleasurable learned if painpattern exists go for easiest if no pain no pleasure random 
         *  depending on the existing stories if no existing stories are there get input if you are tired of that go random until you get enough
         * ____
         * first time:
         * 1. first thread will get the input but when it tries to extract objects from it  it will fail.
         * 2. second thread will fail to extract any object from the input and will fail to extract any story.
         * 3. no interest will be found since there is no objects
         * 
         * second time:
         * 1. first thread will get the input but when it tries to extract objects from it  it will fail.again
         * 2. second thread will possibly be able to extract objects but fail at the stories again ,the extracted objects must be linked to a judging rules
         * 3. it can focuse only on the extracted objects 
         * 
         * third time:
         * 1. first thread will get the input  when it tries to extract objects from it, it can succeed.
         * 2. second thread will possibly be able to extract objects and stories
         * 3. it can focus  on the end of stories objects and compare it with the objects
         * 
         * 
         */
        static void Main(string[] args)
        {
            //start by deserializing the old objects that are in the ListOfSaving made of all the list of inputs and atribut
            
           
            FileStream FileS2 = File.OpenRead(@"C:\Users\user\Desktop\guess.bin");
            ListOfGetting = Serializer.Deserialize<List<Thing>>(FileS2);

            //desirializing orders too

            //while it has energy 
            while (SystemEnergy != 0)
            {   //the idea of using thread is that the input and the evaluation of it should be always running while the pattern finding is happenning of the frames
                // in this thread all the operations of the consciousness should happen  the other thre
                Thread eyeCameraThread = new Thread(new ThreadStart(eyeCamera));
                eyeCameraThread.Start();
                //if patterns thread could find a pattern in the output of the input Analysis thread
                //getting meaning from it

                //this meaning gives a resut as another object whether it is an action generator or not
                SystemEnergy--;
            }
            //if it's not epmty
            // List<Thing> organizedlist=List2.OrderByDescending(o=>o.value).ToList();

            //input
            
            //the input is  a thread because it always sending to the  bashirnadir to give us output
            //the output is a field of interest that we take the interest from which can be a move

            Thing interest = Focus_area.First();//the interst will later compete with reality in imagination and  from it desires imerges to move from the most valuable paattern story root




            // Kill all threads and exit applisecation
            

            using (FileStream fileS1 = File.Open(@"C:\Users\user\Desktop\guess.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                for (int i = 0; i < 3; i++)
                {
                    FillList(1, 3);
                    Serializer.SerializeWithLengthPrefix(fileS1, List1[i], PrefixStyle.Base128, 1);
                }
            }
        }
        private static void bashirNadir()
        {
            //this method is for organizing the Focus_area
            //descision go for the one who has the most pleasurable certain consequence
            Thing relatedPatternedObject = descisionTree(foundInEYE);
            Focus_area.Add(relatedPatternedObject);
        }
        private static Thing descisionTree(List<Thing> foundInEYE)
        {
            throw new NotImplementedException();
        }

        public static void eyeCamera()
        {//happens in main and associated with a thing.
         //in this place the thread must do:
         //the caputer
         //the analysis
            bool TheInputIsStillGiving = true;
            while (TheInputIsStillGiving)
            {
                Thing given = new Thing();
                foundInEYE.Add(given);
            }

            //if earthread and eyethread have return make let the bashirNadir give you what's good from the last discovered in them
            Thread BashirNadirThrad = new Thread(new ThreadStart(bashirNadir));
            int areYouSayingTimerForSlowingDown = 0;
            Thread.Sleep(areYouSayingTimerForSlowingDown);




         
            //the analysis
            capfile = CaptureScreen.CaptureDesktopWithCursor();
         //   capfile.Save(@"C:\Users\user\Desktop\break\img.png", System.Drawing.Imaging.ImageFormat.Png);

            extractGrayScales(capfile);
            //we should test if the pixels in the story list fit into what predefined feeling sum it to give the feeling to the object
           //to do there must be a serialization here for the object we should also make a list of the objects for the using it now
          
        }
        public static void FillList(int value, int pain) => List1.Add(new Thing( value,  pain));

        [ProtoContract]
        public class Thing
        {

            [ProtoMember(1)]
            public int id { get; set; }
            //list<list<int>> usuallyconstructedby//by frame and time
            [ProtoMember(2)]
            public int value { get; set; }//increased or decreased by experience //stored
            
            //the problem with not including motivation is that we can stick to a neuron forever which is a bad thing because something feels the best doesn't have to be repeated forever, or is it ? no, it's not. reality is what gives what to choose we don't have to go through useless mind imaginations.that's why imagination is separated.
            //occurence leads to sticking to bad habbits instead we used statistics by making pain and value a list too for each object takes more memory but yeah better.
            [ProtoMember(4)]
            public int pain { get; set; }
            //things that require more energy get more pain because it will be too expensive for the goal to motivate
            //I never get bored once I expect good from something,(not alway the case I know study is good but don't want to study) I can do it for life
            // why would I stop drinking wine if it has no bad side effects
            //this pain is the experienced pain sometimes pain and enjoyment can coexist (like the saying "why does it feel so good but hurt so bad")
            // a razor is bad if you accidently cut skin but good for shaving so it will be helpful to know that using razor is mostly bad to know it's dangerous but also good to know that it has good uses that why pain is seprated from value, when something gives desired results it's not that bad even it's painful

            List<List<List<List<List<int>>>>> storyOfPixelslistClass = new List<List<List<List<List<int>>>>>();//initialize this when you use the threads in its method
            public static List<List<List<List<List<int>>>>> directionlist = new List<List<List<List<List<int>>>>>();
            public Thing() { }
            public Thing(int value, int pain)
            {
                this.value = value;
                this.pain = pain;
            }


        }

    }
}


//when same thing keeps trying to be sent to the leng term memory and it shouldn't here bordom hits then it should look for the next features. and expending sizes. what can be changed


//two things that define courage the ammount of energy that flows and the amount of pain that blocks.
// pain is seriouse when dealing with what shouldn't be dealt with by patterened ideas