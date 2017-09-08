using System;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Controls;

using Microsoft.Kinect;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;

namespace KinectTicTacToe
{

    public partial class MainWindow : Window
    {

        private bool inGame = false;
        private TextBlock[,] textBlockGrid = new TextBlock[3, 3];

        private KinectSensor kinect = null;
        private KinectAudioStream kinectAudioStream = null;

        private SpeechRecognitionEngine speechEngine = null;

        public MainWindow()
        {
            kinect = KinectSensor.GetDefault();
            kinect.Open();

            IReadOnlyList<AudioBeam> audioBeamList = this.kinect.AudioSource.AudioBeams;
            System.IO.Stream audioStream = audioBeamList[0].OpenInputStream();

            this.kinectAudioStream = new KinectAudioStream(audioStream);

            RecognizerInfo ri = TryGetKinectRecognizer();

            if (null != ri)
            {

                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                Choices boxes = new Choices();
                boxes.Add(new SemanticResultValue("start", "START"));
                boxes.Add(new SemanticResultValue("restart", "RESTART"));

                boxes.Add(new SemanticResultValue("top left", "TOPLEFT"));
                boxes.Add(new SemanticResultValue("top left corner", "TOPLEFT"));
                boxes.Add(new SemanticResultValue("upper left corner", "TOPLEFT"));
                boxes.Add(new SemanticResultValue("top", "TOP"));
                boxes.Add(new SemanticResultValue("top center", "TOP"));
                boxes.Add(new SemanticResultValue("top right", "TOPRIGHT"));
                boxes.Add(new SemanticResultValue("top right corner", "TOPRIGHT"));
                boxes.Add(new SemanticResultValue("upper right corner", "TOPRIGHT"));

                boxes.Add(new SemanticResultValue("left", "LEFT"));
                boxes.Add(new SemanticResultValue("center left", "LEFT"));
                boxes.Add(new SemanticResultValue("center", "CENTER"));
                boxes.Add(new SemanticResultValue("middle", "CENTER"));
                boxes.Add(new SemanticResultValue("right", "RIGHT"));
                boxes.Add(new SemanticResultValue("center right", "RIGHT"));

                boxes.Add(new SemanticResultValue("bottom left", "BOTTOMLEFT"));
                boxes.Add(new SemanticResultValue("bottom left corner", "BOTTOMLEFT"));
                boxes.Add(new SemanticResultValue("lower left corner", "BOTTOMLEFT"));
                boxes.Add(new SemanticResultValue("bottom", "BOTTOM"));
                boxes.Add(new SemanticResultValue("bottom center", "BOTTOM"));
                boxes.Add(new SemanticResultValue("bottom right", "BOTTOMRIGHT"));
                boxes.Add(new SemanticResultValue("bottom right corner", "BOTTOMRIGHT"));
                boxes.Add(new SemanticResultValue("lower right corner", "BOTTOMRIGHT"));

                var gb = new GrammarBuilder { Culture = ri.Culture };
                gb.Append(boxes);

                var g = new Grammar(gb);
                this.speechEngine.LoadGrammar(g);

                this.speechEngine.SpeechRecognized += this.SpeechRecognized;

                this.kinectAudioStream.SpeechActive = true;
                this.speechEngine.SetInputToAudioStream(
                    this.kinectAudioStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                this.speechEngine.RecognizeAsync(RecognizeMode.Multiple);

            }
            else
            {
                Application.Current.Shutdown();
            }

            InitializeComponent();

            textBlockGrid[0, 0] = topLeft;
            textBlockGrid[0, 1] = top;
            textBlockGrid[0, 2] = topRight;

            textBlockGrid[1, 0] = left;
            textBlockGrid[1, 1] = center;
            textBlockGrid[1, 2] = right;

            textBlockGrid[2, 0] = bottomLeft;
            textBlockGrid[2, 1] = bottom;
            textBlockGrid[2, 2] = bottomRight;

        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= 0.35)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "START":
                        if (inGame == false)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {

                                    textBlockGrid[i, j].Text = "";
                                    textBlockGrid[i, j].Foreground = System.Windows.Media.Brushes.Black;
                                }
                            }
                            inGame = true;
                            resultbox.Text = "Playing... Say 'Restart' to Restart";
                            AITurn();
                        }
                        break;
                    case "RESTART":
                        if (e.Result.Confidence >= 0.55)
                        {
                            if (inGame == true)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    for (int j = 0; j < 3; j++)
                                    {

                                        textBlockGrid[i, j].Text = "";
                                        textBlockGrid[i, j].Foreground = System.Windows.Media.Brushes.Black;
                                    }
                                }
                                AITurn();
                            }
                        }
                        break;
                    case "TOPLEFT":
                        if (inGame == true)
                            HumanTurn(0, 0);
                        break;
                    case "TOP":
                        if (inGame == true)
                            HumanTurn(0, 1);
                        break;
                    case "TOPRIGHT":
                        if (inGame == true)
                            HumanTurn(0, 2);
                        break;
                    case "LEFT":
                        if (inGame == true)
                            HumanTurn(1, 0);
                        break;
                    case "CENTER":
                        if (inGame == true)
                            HumanTurn(1, 1);
                        break;
                    case "RIGHT":
                        if (inGame == true)
                            HumanTurn(1, 2);
                        break;
                    case "BOTTOMLEFT":
                        if (inGame == true)
                            HumanTurn(2, 0);
                        break;
                    case "BOTTOM":
                        if (inGame == true)
                            HumanTurn(2, 1);
                        break;
                    case "BOTTOMRIGHT":
                        if (inGame == true)
                            HumanTurn(2, 2);
                        break;
                }
            }
        }
        void AITurn()
        {
            Random r = new Random();
            while (true)
            {
                int row = r.Next(0, 3);
                int col = r.Next(0, 3);

                if (textBlockGrid[row, col].Text == "")
                {
                    textBlockGrid[row, col].Text = "X";
                    break;
                }
            }
            if (CheckIfWin("X") == true)
            {
                inGame = false;
                resultbox.Text = "Oh no! You lost :( Say 'Start' to play again";
                return;
            }
            if (CheckIfTie() == true)
            {
                inGame = false;
                resultbox.Text = "Game is a TIE :/, say 'Start' to play another round";
                return;
            }
        }
        void HumanTurn(int row, int col)
        {

            if (textBlockGrid[row, col].Text == "")
            {
                textBlockGrid[row, col].Text = "O";
                if (CheckIfWin("O") == true)
                {
                    inGame = false;
                    resultbox.Text = "Congrats! You won! Say 'Start' to play again";
                    return;
                }
                if (CheckIfTie() == true)
                {
                    inGame = false;
                    resultbox.Text = "Game is a TIE :/, say 'Start' to play another round";
                    return;
                }
                AITurn();
            }
        }
        private bool CheckIfWin(string v)
        {
            if (textBlockGrid[0, 0].Text == v && textBlockGrid[0, 1].Text == v && textBlockGrid[0, 2].Text == v)
            {

                textBlockGrid[0, 0].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[0, 1].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[0, 2].Foreground = System.Windows.Media.Brushes.Red;
                return true;
            }
            else if (textBlockGrid[1, 0].Text == v && textBlockGrid[1, 1].Text == v && textBlockGrid[1, 2].Text == v)
            {

                textBlockGrid[1, 0].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[1, 1].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[1, 2].Foreground = System.Windows.Media.Brushes.Red;
                return true;
            }
            else if (textBlockGrid[2, 0].Text == v && textBlockGrid[2, 1].Text == v && textBlockGrid[2, 2].Text == v)
            {

                textBlockGrid[2, 0].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[2, 1].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[2, 2].Foreground = System.Windows.Media.Brushes.Red;
                return true;
            }
            else if (textBlockGrid[0, 0].Text == v && textBlockGrid[1, 0].Text == v && textBlockGrid[2, 0].Text == v)
            {

                textBlockGrid[0, 0].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[1, 0].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[2, 0].Foreground = System.Windows.Media.Brushes.Red;
                return true;
            }
            else if (textBlockGrid[0, 1].Text == v && textBlockGrid[1, 1].Text == v && textBlockGrid[2, 1].Text == v)
            {

                textBlockGrid[0, 1].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[1, 1].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[2, 1].Foreground = System.Windows.Media.Brushes.Red;
                return true;
            }
            else if (textBlockGrid[0, 2].Text == v && textBlockGrid[1, 2].Text == v && textBlockGrid[2, 2].Text == v)
            {

                textBlockGrid[0, 2].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[1, 2].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[2, 2].Foreground = System.Windows.Media.Brushes.Red;
                return true;
            }
            else if (textBlockGrid[0, 0].Text == v && textBlockGrid[1, 1].Text == v && textBlockGrid[2, 2].Text == v)
            {

                textBlockGrid[0, 0].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[1, 1].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[2, 2].Foreground = System.Windows.Media.Brushes.Red;
                return true;
            }
            else if (textBlockGrid[2, 0].Text == v && textBlockGrid[1, 1].Text == v && textBlockGrid[0, 2].Text == v)
            {

                textBlockGrid[2, 0].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[1, 1].Foreground = System.Windows.Media.Brushes.Red;
                textBlockGrid[0, 2].Foreground = System.Windows.Media.Brushes.Red;
                return true;
            }
            return false;
        }
        bool CheckIfTie()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (textBlockGrid[i, j].Text == "")
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private static RecognizerInfo TryGetKinectRecognizer()
        {
            IEnumerable<RecognizerInfo> recognizers;

            // This is required to catch the case when an expected recognizer is not installed.
            // By default - the x86 Speech Runtime is always expected. 
            try
            {
                recognizers = SpeechRecognitionEngine.InstalledRecognizers();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return null;
            }

            foreach (RecognizerInfo recognizer in recognizers)
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }
    }
}
