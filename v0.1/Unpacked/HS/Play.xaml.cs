using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using SharpVectors.Converters;
using SharpVectors.Dom;
using SharpVectors.Renderers;
using SharpVectors.Renderers.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Printing;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace HS
{
    /// <summary>
    /// Interaction logic for Play.xaml
    /// </summary>
    public partial class Play : Window
    {
        // moveforward cannot do angles

        void hideFromPlayer(UIElement obj)
        {
            Dispatcher.Invoke(new Action(() => {
                obj.Visibility = Visibility.Hidden;
            }));
        }

        public static int current_scene_index = 0;
        public Play()
        {
            InitializeComponent();
            runPlugin();
        }
        public GlobalVars Game = new GlobalVars();

        public List<message_listner> message_listners = new List<message_listner>();

        public class message_listner
        {
            public enum _type
            {
                Matches = 1,
                Equals = 2
            }
            public _type type { get; set; }
            public string message { get; set; }
            public hs_object parent_object = new hs_object();
            public hs_rule rule = new hs_rule();
        }

        public string message
        {
            get { return _message; }
            set
            {
                _message = value;
                foreach (message_listner listner in message_listners)
                {
                    if (listner.type == message_listner._type.Equals)
                    {
                        if (_message == listner.message)
                        {
                            foreach (hs_ability ability in listner.rule.abilities)
                            {
                                executeBlocks(ability.blocks, listner.parent_object);
                            }
                        }
                    }
                    else if (listner.type == message_listner._type.Matches)
                    {
                        if (Regex.IsMatch(_message, listner.message))
                        {
                            foreach (hs_ability ability in listner.rule.abilities)
                            {
                                executeBlocks(ability.blocks, listner.parent_object);
                            }
                        }
                    }
                }
            }
        }

        private string _message;

        public class hs_scene {
            public string id { get; set; } 
            public string name { get; set; } 
            public List<hs_object> objects = new List<hs_object>();
            public void destroy(hs_object obj)
            {
                var play = new Play();
                play.hideFromPlayer(obj.element);
                this.objects.Remove(obj);
            }
        }
        public class hs_object {
            public string id { get; set; }
            public string name { get; set; }
            public List<hs_rule> rules = new List<hs_rule>();
            public eventData eventData = new eventData(); // special
            public int type { get; set; }
            public dynamic element { get; set; }
            public hs_scene parent { get; set; }
        }
        public class hs_rule { 
            public string id { get; set; } 
            public List<hs_ability> abilities = new List<hs_ability>();
            public int type { get; set; }
        }
        public class hs_ability
        {
            public string id { get; set; }
            public List<hs_block> blocks = new List<hs_block>();
            public int type { get; set; }
        }
        public class hs_block { 
            public string id { get; set; } 
            public int type { get; set; }
        }

        public GlobalVars GetVars()
        {
            return Game;
        }

        public class GlobalVars
        {
            public int current_scene_index {
                get {
                    return Play.current_scene_index;
                }
                set {
                    throw new Exception("current_scene_index cannot be redefined");
                }
            }
            public List<hs_scene> scenes = new List<hs_scene>();

            public blocktypes blocktypes = new blocktypes();

            public enum MessageBoxButton
            {
                // copied directly from definiton
                OK = 0,
                OKCancel = 1,
                YesNoCancel = 3,
                YesNo = 4
            }

            public void alert(string message, string caption, MessageBoxButton buttonOptions)
            {
                var result = MessageBox.Show(message, caption, (System.Windows.MessageBoxButton)buttonOptions);
            }

            public void alert(string message)
            {
                MessageBox.Show(message);
            }
        }

        public async void open_project(object sender, RoutedEventArgs e) {
            await loader.EnsureCoreWebView2Async();
            string[] loading_messages = { "11% of the world is left handed.", "A full head of human hair is strong enough to support 12 tonnes.", "A full moon is nine times brighter than a half moon", "A grizzly bear’s bite could crush a bowling ball.", "A group of pugs is called a grumble.", "A group of turkeys is called a “rafter”.", "A hippopotamus may seem huge but it can still run faster than a man.", "Alexander the Great is said to have camped under a Banyan tree that was big enough to shelter his whole army of 7,000 men.", "Although the Stegosaurus dinosaur was over 9 metres long, its brain was only the size of a walnut.", "An average fly buzzes in the key of F", "Another way to write repeat forever is to set “When” 7 = 7!", "A project cannot be in Trending and Featured at the same time. (OOP)", "Around 90% of the cells that make humans aren’t “human” in origin. We’re mostly fungi and bacteria.", "Astronauts can grow up to 2 inches because of the lack of pressure put on their spine.", "At Hopscotch we like to give our dead batteries away … free of charge.", "A wildebeest can run away from predators 5 minutes after being born.", "Beethoven dipped his head in cold water before he composed.", "Chillanna is from Yukon. Her wheeled snowboard is the only one of its kind, able to ride on both snow and land.", "Cupcake and Mr. Mustache used to date.", "Cupcake was originally baked in a bakery in San Diego.", "Did you hear about the two guys who stole the calendar? They each got six months.", "Did you hear about the mathematician who hates negative numbers? He’ll stop at nothing to avoid them.", "Did you know a cat has 32 muscles in each ear?", "Did You Know: Emoji is actually the long lost language from the island of Emoja.", "Don’t trust an atom, they make up everything.", "Earth’s axis has a wobble that takes 26,000 years.", "Footprints and tyre tracks left behind by astronauts on the moon will stay there - there is no wind to blow them away.", "Ghost ants become the color of the food they are eating.", "Gorilla’s coloring is due to an accidental swim in a vat of grape soda.", "Go to your iPad settings >> keyboard >> languages to enable emojis.", "Hedy Lamarr was a beautiful movie star AND she invented the technology that lead to cell phones.", "Hopscotch submitted ten puns to a joke contest hoping one would win. Sadly, no pun in ten did.", "Human fingers are so sensitive, that if your fingers were the size of the Earth, you could feel the difference between a house and a car.", "Humans share 50% of their DNA with bananas.", "Humans shed 40 pounds of skin in their lifetime, completely replacing their outer skin every month.", "If the human brain were a computer, it could perform 38 thousand-trillion operations per second. The most powerful supercomputers, manage only 0.002% of that!", "If you want followers to make your username easy to read; use a normal font and no emojis until the end!", "If you want to change the ordering of blocks, you “Send to Back” of “Bring to Front” blocks", "If you want your character to face left use the “Flip” function in the “Movement” menu.", "I just came up with a new word: plagiarism!", "In 30 minutes, the human body gives off enough heat to bring a gallon of water to the boil.", "I read a book about anti-gravity. It was impossible to put down!", "It’s hard to explain puns to kleptomaniacs because they are always taking things literally.", "I used to hate facial hair. But then it grew on me.", "I was going to tell a sodium hydrogen pun … but NaH", "I wondered why the baseball was getting bigger. Then it hit me.", "Japanese cop cars fire paintballs at speeding cars to find them later in case they get away.", "King Tut was 10 when he became king of Egypt.", "LASER is an acronym that stands for Light Amplification by Stimulated Emission of Radiation", "Like fingerprints, everyone’s tongue print is different.", "Make your object invisible by setting invisibility to 100%", "More than 80% of the world’s population lives in the northern hemisphere.", "Mosquito is half the size of the human characters because he is prehistoric. She was excavated from amber in 1993.", "Mr. Mustache used magic to escape the illuminati a couple years ago.", "Octopuses and Elephants have been observed to have funerals for their fallen comrades.", "On average the Atlantic Ocean is the saltiest of Earth’s major oceans.", "Otters sleep holding hands.", "Palindromes are spelled the same way forward and backward! Some examples are “race car” or “taco cat”", "Parallel lines have so much in common it’s a shame they’ll never meet.", "Polar bears have black skin under their fur.", "Raccoon escaped from a fur factory when he was a cub.", "Sign up for notifications to know when someone remixes your project!", "Sneezing with your eyes open is impossible.", "Someone stole my mood ring. i don’t know how I feel about it.", "Some women see more colors than everyone else. They have 4 or 5 color receptors, while most people have only 3.", "Star a Girl was a pop star in Europe but stopped touring so she could build worlds in Hopscotch.", "There are enough veins in the human circulatory system to circle the earth 2.5 times!", "The default speed of any character before you change it is 400!", "The default value for any new variable you create is 0 before you change it.", "The Earth isn’t perfectly round, it is slightly flattened at the north and south poles.", "The first coins were minted (made) around 2500 years ago.", "The sun’s closest neighbor, Proxima Centauri, is 4.24 lightyears away.", "The last object Sent to Back will be at the very back. The last object Brought to Front will be in the very front.", "The length of the squares in the editor grid is 50!", "The redness of a sunburn is chased by groups of burst blood vessels near the surface of the skin.", "The world’s largest desert (outside of the polar regions) is the Sahara, it covers about one third of Africa.", "The world’s largest volcano is sleeping underneath Yellowstone National Park.", "The volcanic rock known as pumice is the only rock that can float in water.", "This app is called Hopscotch because, like the street game, you have to make it before you can play it.", "Though seemingly exotic to Americans, the Venus flytrap originated in the wetlands of North and South Carolina.", "To paint the entire screen a color Leave a Trail of Width bigger than 3000 and Move forward 1.", "Two mice were chewing on a film roll. One of them goes: “I think the book was better.”", "We humans are the best long-distance runners on the planet. Better than any four-legged animal. In fact, thousands of years ago we used to run after our prey until they died of exhaustion.", "We think this dancing bear will entertain you while your project is loading.", "We were going to do a joke about time travel but you didn’t like it.", "What did the ocean say to the sand? Nothing, it just waved", "What does an angry pepper do? It gets jalapeño face.", "What do you call a fake noodle. An impasta.", "What do you call a fish with no eye? A fsh.", "What do you get when you cross a bridge with a car? To the other side.", "When life gives you lemons, make orange juice and let them wonder how you did it.", "When smiley emojis die, they become the ghost emoji.", "When swimming, elephants use their trunk to breathe like a snorkel.", "Why are mountains so funny? Because they are hill areas!", "Why did the bear cross the road? Someone changed his X.", "Why did the can crusher quit his job? Because it was soda-pressing.", "Why is Yoda afraid of seven? Because six seven eight!", "Why was Pavlov’s hair so soft? Classical conditioning!", "With 3 quarters, 4 dimes, and 4 pennies, you would have $1.19 but not able to make change for a dollar.", "You are born with 300 bones, by the time you are an adult you will have 206.", "You breathe on average about 5 million times per year.", "You can create custom blocks if you want to reuse your code!", "You can delete or rename custom blocks by holding down on the keyboard hey.", "You can get more information about each block by holding down the key!", "You have 2 meters of DNA per cell.", "Your body produces 25 million new cells each second. Every 13 seconds, you produce more cells than there are people in the United States.", "You’re a little richer than you might think, inside all of us is around 0.2 milligrams of gold, most of which is in our blood." };
            Random random = new Random();
            int index = random.Next(0, loading_messages.Length);
            string loading_message = loading_messages[index];
            string _uuid = uuid.Text;
            loader.NavigateToString(@"<!DOCTYPE html><html><head><style> /* 3D tower loader made by: csozi | Website: www.csozi.hu*/.loader { scale: 3; height: 50px; width: 40px;}.box { position: relative; opacity: 0; left: 10px;}.side-left { position: absolute; background-color: #286cb5; width: 19px; height: 5px; transform: skew(0deg, -25deg); top: 14px; left: 10px;}.side-right { position: absolute; background-color: #2f85e0; width: 19px; height: 5px; transform: skew(0deg, 25deg); top: 14px; left: -9px;}.side-top { position: absolute; background-color: #5fa8f5; width: 20px; height: 20px; rotate: 45deg; transform: skew(-20deg, -20deg);}.box-1 { animation: from-left 4s infinite;}.box-2 { animation: from-right 4s infinite; animation-delay: 1s;}.box-3 { animation: from-left 4s infinite; animation-delay: 2s;}.box-4 { animation: from-right 4s infinite; animation-delay: 3s;}@keyframes from-left { 0% { z-index: 20; opacity: 0; translate: -20px -6px; } 20% { z-index: 10; opacity: 1; translate: 0px 0px; } 40% { z-index: 9; translate: 0px 4px; } 60% { z-index: 8; translate: 0px 8px; } 80% { z-index: 7; opacity: 1; translate: 0px 12px; } 100% { z-index: 5; translate: 0px 30px; opacity: 0; }}@keyframes from-right { 0% { z-index: 20; opacity: 0; translate: 20px -6px; } 20% { z-index: 10; opacity: 1; translate: 0px 0px; } 40% { z-index: 9; translate: 0px 4px; } 60% { z-index: 8; translate: 0px 8px; } 80% { z-index: 7; opacity: 1; translate: 0px 12px; } 100% { z-index: 5; translate: 0px 30px; opacity: 0; }}.centre {position: absolute; top:0; bottom: 0; left: 0; right: 0; margin: auto;}</style></head><body><div class=""loader centre""> <div class=""box box-1""> <div class=""side-left""></div> <div class=""side-right""></div> <div class=""side-top""></div> </div> <div class=""box box-2""> <div class=""side-left""></div> <div class=""side-right""></div> <div class=""side-top""></div> </div> <div class=""box box-3""> <div class=""side-left""></div> <div class=""side-right""></div> <div class=""side-top""></div> </div> <div class=""box box-4""> <div class=""side-left""></div> <div class=""side-right""></div> <div class=""side-top""></div> </div></div><div class=""centre"" style=""width:fit-content;height:fit-content; top: 200px; font-family: arial;"">" + loading_message + "</div></body></html>");
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://c.gethopscotch.com/api/v1/projects/" + _uuid);
            var response = await client.SendAsync(request);
            dynamic test = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            string output = "";
            // Scenes
            for (int a = 0; a < test.scenes.Count; a++)
            {
                hs_scene _scene = new hs_scene { id = System.Guid.NewGuid().ToString(), name = test.scenes[a].name };
                Game.scenes.Add(_scene);

                // Objects

                for (int b = 0; b < test.scenes[a].objects.Count; b++)
                {
                    // Loop through objects to find correct one
                    for (int c = 0; c < test.objects.Count; c++)
                    {
                        if (test.objects[c].objectID == test.scenes[a].objects[b])
                        {
                            string objectId = System.Guid.NewGuid().ToString();
                            string text = "";
                            if (test.objects[c].ContainsKey("text")) { text = test.objects[c].text; }
                            hs_object _object = new hs_object { id = objectId, name = test.objects[c].name, type = test.objects[c].type, element = addElement((decimal)test.objects[c].xPosition, (decimal)test.objects[c].yPosition, (int)test.objects[c].type, objectId, text) };
                            _object.parent = _scene;
                            _scene.objects.Add(_object);

                            // Loop through rules (purple blocks) - for each rule in object
                            for (int d = 0; d < test.objects[c].rules.Count; d++)
                            {
                                // cant use variable e so skip it: d -> f
                                // Loop through *standard* rules (purple blocks) to find correct one
                                for (int f = 0; f < test.rules.Count; f++)
                                {
                                    if (test.rules[f].id == test.objects[c].rules[d])
                                    {
                                        // Add rules's block type
                                        string ruleID = System.Guid.NewGuid().ToString();
                                        hs_rule _rule = new hs_rule { id = System.Guid.NewGuid().ToString(), type = test.rules[f].parameters[0].datum.type };
                                        _object.rules.Add(_rule);

                                        bool isGameStarts = false;
                                        if (_rule.type == (int)blocktypes.When.GameStarts) { isGameStarts = true; }

                                        for (int g = 0; g < test.abilities.Count; g++)
                                        {
                                            if (test.abilities[g].abilityID == test.rules[f].abilityID)
                                            {
                                                hs_ability _ability = new hs_ability { id = System.Guid.NewGuid().ToString() };
                                                _rule.abilities.Add(_ability);

                                                // list every block
                                                for (int h = 0; h < test.abilities[g].blocks.Count; h++)
                                                {
                                                    hs_block _block = new hs_block { id = System.Guid.NewGuid().ToString(), type = test.abilities[g].blocks[h].type };
                                                    _ability.blocks.Add(_block);
                                                }

                                                if (isGameStarts == true)
                                                {
                                                    executeBlocks(_ability.blocks, _object);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            executeConstants();
            loader.Visibility = Visibility.Collapsed;
        }

        public class blocktypes
        {
            public enum When
            {
                GameStarts = 7000, // done
                IsTapped = 7001, // done
                IsTouching = 7002,
                IsPressed = 7003, // done
                TiltedRight = 7004, // done
                TiltedLeft = 7005, // done
                TiltedUp = 7006, // done
                TiltedDown = 7007, // done
                LoudNoise = 7008, // done
                IsShaken = 7009, // done
                Bumps = 7010,
                IsSwipedRight = 7011, // done
                IsSwipedLeft = 7012, // done
                IsSwipedUp = 7013, // done
                IsSwipedDown = 7014, // done
                Cloned = 7015, // done - invoke when block is executed
                IsNotPressed = 7020, // done
                IsPlaying = 7021, // done
                TouchEnds = 7022,
                GetMessage = 7023, // done
                MessageMatches = 7024, // done
                IsNotTouching = 7025
            }

            public enum Movement
            {
                MoveForward = 23, // done
                Turn = 24, // done
                ChangeX = 27, // done
                ChangeY = 28, // done
                SetSpeed = 34, // done
                SetAngle = 39, // done
                SetPositon = 41, // done
                Flip = 50, // done - not tested
                SetOrigin = 59,
                SetCenter = 60
            }

            public enum Looks
            {
                ChangePose = 33,
                SetText = 40,
                SendToBack = 42,
                BringToFront = 43,
                SetInvisibility = 47,
                GrowBy = 48,
                ShrinkBy = 49,
                SetSize = 51,
                SetColor = 54,
                SetImage = 56,
                SetWidthAndHeight = 57,
                SetZIndex = 58,
                StartSound = 62,
                SetTextToInput = 64,
                PlayNote = 65,
                SetTempo = 66,
                SetInstrument = 67,
                SetBackground = 70,
                ShowPopup = 72
            }

            public enum Control
            {
                WaitMilliseconds = 35,
                CreateAClone = 53,
                Destroy = 55,
                WaitSeconds = 61,
                OpenProject = 68,
                Comment = 69,
                RepeatTimes = 120,
                RepeatForever = 121,
                CheckOnceIf = 122,
                CheckIfElse = 124,
                ChangeScene = 125,
                BroadcastMessage = 126,
                RequestSeeds = 127
            }

            public enum Trails
            {
                DrawATrail = 26,
                Clear = 30,
                SetTrailWidth = 31,
                SetTrailColor = 32,
                SetTrailCap = 71,
                SetTrailOpacity = 73
            }

            public enum Variables
            {

            }

            public enum Special
            {
                isDesktop = 101010
            }
        }

        //List<hs_object> objects = new List<hs_object>();

        List<hs_object> objects_withnotpressed = new List<hs_object>();

        //public class hs_object
        //{
        //    public string id { get; set; }
        //    public int type { get; set; }
        //    public List<rule> rules = new List<rule>();
        //    public eventData eventData = new eventData();
        //}

        public class eventData
        {
            public bool touch = false;
            public Point SwipeStart = new Point { X = -100, Y = -100 };
            public int speed = 1000;
        }

        public class rule
        {
            public int type { get; set; }
            public List<block> blocks = new List<block>();
        }

        public class block
        {
            public int type { get; set; }
        }

        public async void executeBlocks(List<hs_block> blocks, hs_object obj)
        {
            new Thread(async () =>
            {
                var test = (TextBlock)obj.element;
                this.Dispatcher.Invoke(() =>
                {
                    obj.element.Text = "hello world";
                });
                foreach (hs_block block in blocks)
                {
                    List<dynamic> parameters = new List<dynamic>();
                    parameters.Add(50);

                    switch (block.type)
                    {
                        case (int)blocktypes.Movement.MoveForward:
                            //// 1000 pixels per min
                            //int speed = 60 / obj.eventData.speed;
                            int distance = parameters[0];

                            for (int i = 0; i < Math.Abs(distance); i++)
                            {
                                await moveForward(obj, 1000, isPositive(distance));
                            }
                            break;

                        case (int)blocktypes.Movement.Turn:
                            RotateTransform _rotation = obj.element.RenderTransform as RotateTransform;
                            if (_rotation != null)
                            {
                                double rotation = _rotation.Angle;
                                RotateTransform a = new RotateTransform(rotation + parameters[0]);
                                obj.element.RenderTransform = a;
                            }
                            else
                            {
                                RotateTransform a = new RotateTransform(parameters[0]);
                                obj.element.RenderTransform = a;
                            }
                            break;

                        case (int)blocktypes.Movement.SetAngle:
                            RotateTransform b = new RotateTransform(parameters[0]);
                            obj.element.RenderTransform = b;
                            break;

                        case (int)blocktypes.Movement.SetSpeed:
                            obj.eventData.speed = parameters[0];
                            break;

                        case (int)blocktypes.Movement.SetPositon:
                            obj.element.Margin = new Thickness(parameters[0], parameters[1], 0, 0);
                            break;

                        case (int)blocktypes.Movement.Flip:
                            ScaleTransform current = obj.element.RenderTransform;
                            if (current.ScaleX != -1) {
                                ScaleTransform sc = new ScaleTransform();
                                sc.ScaleX = -1;
                                obj.element.RenderTransform = sc;
                            } else {
                                ScaleTransform sc = new ScaleTransform();
                                sc.ScaleX = 0;
                                obj.element.RenderTransform = sc;
                            }
                            break;

                        case (int)blocktypes.Movement.ChangeX:
                            //// 1000 pixels per min
                            //int speed = 60 / obj.eventData.speed;
                            int _distance = parameters[0];

                            for (int i = 0; i < Math.Abs(_distance); i++)
                            {
                                await changex(obj, 1000, isPositive(_distance));
                            }
                            break;

                        case (int)blocktypes.Movement.ChangeY:
                            //// 1000 pixels per min
                            //int speed = 60 / obj.eventData.speed;
                            int __distance = parameters[0];

                            for (int i = 0; i < Math.Abs(__distance); i++)
                            {
                                await changey(obj, 1000, isPositive(__distance));
                            }
                            break;

                        default:
                            break;
                    }
                }
            }).Start();
        }

        private bool isPositive(int num)
        {
            if (num < 0) return false; return true;
        }

        public async Task moveForward(hs_object obj, int speed, bool forwards)
        {
            double distance = speed * 0.02;
            this.Dispatcher.Invoke(async () =>
            {
                if (forwards)
                {
                    obj.element.Margin = new Thickness(obj.element.Margin.Left + distance, obj.element.Margin.Top, 0, 0);
                }
                else
                {
                    obj.element.Margin = new Thickness(obj.element.Margin.Left - distance, obj.element.Margin.Top, 0, 0);
                }
            });
            await Task.Delay(16);
        }

        public async Task changex(hs_object obj, int speed, bool forwards)
        {
            double distance = speed * 0.02;
            this.Dispatcher.Invoke(async () =>
            {
                if (forwards)
                {
                    obj.element.Margin = new Thickness(obj.element.Margin.Left + distance, obj.element.Margin.Top, 0, 0);
                }
                else
                {
                    obj.element.Margin = new Thickness(obj.element.Margin.Left - distance, obj.element.Margin.Top, 0, 0);
                }
            });
            await Task.Delay(16);
        }

        public async Task changey(hs_object obj, int speed, bool forwards)
        {
            double distance = speed * 0.02;
            this.Dispatcher.Invoke(async () =>
            {
                if (forwards)
                {
                    obj.element.Margin = new Thickness(obj.element.Margin.Left, obj.element.Margin.Top + distance, 0, 0);
                }
                else
                {
                    obj.element.Margin = new Thickness(obj.element.Margin.Left, obj.element.Margin.Top - distance, 0, 0);
                }
            });
            await Task.Delay(16);
        }

        public void executeMethods(List<string> methods)
        {
            foreach (string a in methods)
            {
                try
                {
                    MethodInfo b = this.GetType().GetMethod(a);
                    b.Invoke(this, null);
                }
                catch (System.NullReferenceException e)
                {
                    MessageBox.Show($"ERROR: Method Not Found\n\nException Details:\n{e.ToString()}");
                }
            }
        }

        public void mouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement a)
            {
                bool touch = false;
                hs_object obj = null;
                List<hs_object> objects = new List<hs_object>();
                for (int b = 0; b < Game.scenes.Count; b++)
                {
                    obj = Game.scenes[b].objects.FirstOrDefault(c => c.id == a.Tag.ToString());
                    if (obj != null)
                    {
                        objects = Game.scenes[b].objects;
                        break;
                    }
                }
                if (obj != null)
                {
                    obj.eventData.touch = true;
                    int i = 0;
                    foreach (hs_rule rule in obj.rules) // run tap
                    {
                        switch ((blocktypes.When)rule.type)
                        {
                            case blocktypes.When.IsTapped:
                                foreach (hs_ability ability in obj.rules[i].abilities)
                                {
                                    Parallel.ForEach(ability.blocks, block =>
                                    { //run each block
                                        MessageBox.Show(block.type.ToString());
                                    });
                                }
                                break;
                        }
                        i++;
                    }
                }
            }
        }

        public void mouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement a)
            {
                hs_object obj = null;
                List<hs_object> objects = new List<hs_object>();
                for (int b = 0; b < Game.scenes.Count; b++)
                {
                    obj = Game.scenes[b].objects.FirstOrDefault(c => c.id == a.Tag.ToString());
                    if (obj != null)
                    {
                        objects = Game.scenes[b].objects;
                        break;
                    }
                }
                if (obj != null)
                {
                    obj.eventData.touch = false;
                }
            }
        }

        public void Object_Cloned(hs_object obj)
        {
            foreach (hs_rule rule in obj.rules.FindAll(a => a.type == (int)blocktypes.When.Cloned))
            {
                foreach (hs_ability ability in rule.abilities)
                {
                    executeBlocks(ability.blocks, obj);
                }
            }
        }

        public void Simulate_LoudNoise(object sender, RoutedEventArgs e)
        {
            foreach (hs_object obj in Game.scenes[current_scene_index].objects)
            {
                foreach (hs_rule rule in obj.rules.FindAll(a => a.type == (int)blocktypes.When.LoudNoise))
                {
                    foreach (hs_ability ability in rule.abilities)
                    {
                        executeBlocks(ability.blocks, obj);
                    }
                }
            }
        }

        public void Simulate_Shake(object sender, RoutedEventArgs e)
        {
            foreach (hs_object obj in Game.scenes[current_scene_index].objects)
            {
                foreach (hs_rule rule in obj.rules.FindAll(a => a.type == (int)blocktypes.When.IsShaken))
                {
                    foreach (hs_ability ability in rule.abilities)
                    {
                        executeBlocks(ability.blocks, obj);
                    }
                }
            }
        }

        public async Task executeConstants()
        {
                await Task.Run(() =>
                {
                    while (true)
                    {
                        foreach (hs_object obj in Game.scenes[current_scene_index].objects)
                        {
                            foreach (hs_rule rule in obj.rules.FindAll(a => a.type == (int)blocktypes.When.IsNotPressed))
                            {
                                if (obj.eventData.touch == false)
                                {
                                    foreach (hs_ability ability in rule.abilities)
                                    {
                                        executeBlocks(ability.blocks, obj);
                                    }
                                }
                            }
                            foreach (hs_rule rule in obj.rules.FindAll(a => a.type == (int)blocktypes.When.IsPressed))
                            {
                                if (obj.eventData.touch == true)
                                {
                                    foreach (hs_ability ability in rule.abilities)
                                    {
                                        executeBlocks(ability.blocks, obj);
                                    }
                                }
                            }
                            foreach (hs_rule rule in obj.rules.FindAll(a => a.type == (int)blocktypes.When.IsPlaying))
                            {
                                foreach (hs_ability ability in rule.abilities)
                                {
                                    executeBlocks(ability.blocks, obj);
                                }
                            }
                        }

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            if (X_Slider.IsInitialized == true && X_Slider.Value != 0)
                            {
                                if (X_Slider.Value < 0)
                                {
                                    // tilt left
                                    foreach (hs_object obj in Game.scenes[current_scene_index].objects)
                                    {
                                        foreach (hs_rule rule in obj.rules.FindAll(a => a.type == (int)blocktypes.When.TiltedRight))
                                        {
                                            foreach (hs_ability ability in rule.abilities)
                                            {
                                                executeBlocks(ability.blocks, obj);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // tilt right
                                    foreach (hs_object obj in Game.scenes[current_scene_index].objects)
                                    {
                                        foreach (hs_rule rule in obj.rules.FindAll(a => a.type == (int)blocktypes.When.TiltedLeft))
                                        {
                                            foreach (hs_ability ability in rule.abilities)
                                            {
                                                executeBlocks(ability.blocks, obj);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (Y_Slider.IsInitialized == true && Y_Slider.Value != 0)
                            {
                                if (Y_Slider.Value < 0)
                                {
                                    // tilt down
                                    foreach (hs_object obj in Game.scenes[current_scene_index].objects)
                                    {
                                        foreach (hs_rule rule in obj.rules.FindAll(a => a.type == (int)blocktypes.When.TiltedDown))
                                        {
                                            foreach (hs_ability ability in rule.abilities)
                                            {
                                                executeBlocks(ability.blocks, obj);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // tilt up
                                    foreach (hs_object obj in Game.scenes[current_scene_index].objects)
                                    {
                                        foreach (hs_rule rule in obj.rules.FindAll(a => a.type == (int)blocktypes.When.TiltedUp))
                                        {
                                            foreach (hs_ability ability in rule.abilities)
                                            {
                                                executeBlocks(ability.blocks, obj);
                                            }
                                        }
                                    }
                                }
                            }
                        }));
                    }
                });
        }

        dynamic addElement(decimal x, decimal y, int elementType, string elementID, string text = "")
        {
            dynamic output = null;
            double ypos = playerBorder.ActualHeight - (double)y;
            if (y < 0)
            {
                ypos = (2 * (0 - (double)y));
            }
            switch (elementType)
            {
                case 1: //text
                    var element = new TextBlock();
                    element.Text = text;
                    element.Margin = new Thickness(Convert.ToDouble(x), ypos, 0, 0);
                    element.MouseDown += new MouseButtonEventHandler(mouseDown);
                    element.MouseUp += new MouseButtonEventHandler(mouseUp);
                    element.Tag = elementID;
                    element.RenderTransformOrigin = new Point(0.5, 0.5);
                    element.MouseDown += new MouseButtonEventHandler(element_MouseDown);
                    element.MouseMove += new MouseEventHandler(element_MouseMove);
                    output = element;
                    player.Children.Add(element);
                    break;
                case var val when (val == 0 || (val > 1 && val < 3003)):
                    var path = new SvgViewbox();
                    path.Source = new Uri(System.AppDomain.CurrentDomain.BaseDirectory + "assets/svgexport-" + val + ".svg");
                    path.Margin = new Thickness(Convert.ToDouble(x), ypos, 0, 0);
                    path.MouseDown += new MouseButtonEventHandler(mouseDown);
                    path.MouseUp += new MouseButtonEventHandler(mouseUp);
                    path.Tag = elementID;
                    path.RenderTransformOrigin = new Point(0.5, 0.5);
                    path.MouseDown += new MouseButtonEventHandler(element_MouseDown);
                    path.MouseMove += new MouseEventHandler(element_MouseMove);
                    path.SetValue(Pose, "0");
                    output = path;
                    player.Children.Add(path);
                    hs_object elementToAdd = new hs_object { id = elementID };
                    break;
                default:
                    break;
            }
            return output;
        }

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Y_Slider.IsInitialized == true && X_Slider.IsInitialized == true)
            {
                double y = 0-Y_Slider.Value;
                double x = 0-X_Slider.Value;
                string html = @"<!DOCTYPE html><html><head><style>.device{transform:rotateX(" + y.ToString() + @"deg) rotateY(" + x.ToString() + @"deg);width:80%;height:auto;position:absolute;top:0;left:0;bottom:0;right:0;margin:auto}</style></head><body><img src=""https://www.apple.com/newsroom/images/product/ipad/standard/apple_ipados14_widgets_062220_big.jpg.large.jpg"" class=""device""></body></html>";
                tilt_preview.NavigateToString(html);
            }
        }

        private void show_popup(object sender, RoutedEventArgs e)
        {
            if(!tilt_popup.IsOpen) tilt_popup.IsOpen = true;
            else tilt_popup.IsOpen = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (tilt_popup.IsOpen)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        Y_Slider.Value += 10;
                        break;
                    case Key.Down:
                        Y_Slider.Value -= 10;
                        break;
                    case Key.Left:
                        X_Slider.Value -= 10;
                        break;
                    case Key.Right:
                        X_Slider.Value += 10;
                        break;
                }
            }
        }

        private void element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement a)
            {
                hs_object obj = null;
                List<hs_object> objects = new List<hs_object>();
                for (int b = 0; b < Game.scenes.Count; b++)
                {
                    obj = Game.scenes[b].objects.FirstOrDefault(c => c.id == a.Tag.ToString());
                    if (obj != null)
                    {
                        objects = Game.scenes[b].objects;
                        break;
                    }
                }
                if (obj != null)
                {
                    obj.eventData.SwipeStart = e.GetPosition(this);
                }
            }
        }

        public double toPositive(double num)
        {
            if (num < 0) return 0-num;
            else return num;
        }

        private void wahoo(hs_object object_data)
        {
            object_data.parent.destroy(object_data);
        }

        public class Swipe
        {
            public static void Left(hs_object object_data)
            {
                var a = new Play();
                a.wahoo(object_data);
                foreach (hs_rule rule in object_data.rules.FindAll(a => a.type == (int)blocktypes.When.IsSwipedLeft))
                {
                    foreach (hs_ability ability in rule.abilities)
                    {
                        var play = new Play();
                        play.executeBlocks(ability.blocks, object_data);
                    }
                }
            }
            public static void Right(hs_object object_data)
            {
                foreach (hs_rule rule in object_data.rules.FindAll(a => a.type == (int)blocktypes.When.IsSwipedRight))
                {
                    foreach (hs_ability ability in rule.abilities)
                    {
                        var play = new Play();
                        play.executeBlocks(ability.blocks, object_data);
                    }
                }
            }
            public static void Up(hs_object object_data)
            {
                foreach (hs_rule rule in object_data.rules.FindAll(a => a.type == (int)blocktypes.When.IsSwipedUp))
                {
                    foreach (hs_ability ability in rule.abilities)
                    {
                        var play = new Play();
                        play.executeBlocks(ability.blocks, object_data);
                    }
                }
            }
            public static void Down(hs_object object_data)
            {
                foreach (hs_rule rule in object_data.rules.FindAll(a => a.type == (int)blocktypes.When.IsSwipedDown))
                {
                    foreach (hs_ability ability in rule.abilities)
                    {
                        var play = new Play();
                        play.executeBlocks(ability.blocks, object_data);
                    }
                }
            }
        }

        private void element_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement a)
            {
                hs_object obj = null;
                List<hs_object> objects = new List<hs_object>();
                for (int b = 0; b < Game.scenes.Count; b++)
                {
                    obj = Game.scenes[b].objects.FirstOrDefault(c => c.id == a.Tag.ToString());
                    if (obj != null)
                    {
                        objects = Game.scenes[b].objects;
                        break;
                    }
                }
                if (obj != null)
                {
                    if (obj.eventData.SwipeStart != new Point { X = -100, Y = -100 })
                    {
                        double x_diff = obj.eventData.SwipeStart.X - e.GetPosition(this).X;
                        double y_diff = obj.eventData.SwipeStart.Y - e.GetPosition(this).Y;

                        if (toPositive(y_diff) > toPositive(x_diff))
                        {
                            // vertical swipe
                            if (y_diff > 30)
                            {
                                obj.eventData.SwipeStart = new Point { X = -100, Y = -100 };
                                Swipe.Up(obj);
                            }
                            else if (y_diff < -30)
                            {
                                obj.eventData.SwipeStart = new Point { X = -100, Y = -100 };
                                Swipe.Down(obj);
                            }
                        }
                        else
                        {
                            // horizontal swipe
                            if (x_diff > 30)
                            {
                                obj.eventData.SwipeStart = new Point { X = -100, Y = -100 };
                                Swipe.Left(obj);
                            }
                            else if (x_diff < -30)
                            {
                                obj.eventData.SwipeStart = new Point { X = -100, Y = -100 };
                                Swipe.Right(obj);
                            }
                        }
                    }
                }
            }
        }

        public void runPlugin()
        {
            try
            {
                CSharpScript.RunAsync(@"//alert(""Do you want to continue?"", ""Warning"", MessageBoxButton.YesNo);", globals: Game);
            }
            catch (Exception e)
            {
                MessageBox.Show("An exception has been enountered in the plugin\n\n" + e.Message);
            }
        }

        public static readonly DependencyProperty Pose = DependencyProperty.RegisterAttached("Pose", typeof(string), typeof(Window));

        public enum _pose
        {
            text = 0,
            image = 1
        }
    }
}
