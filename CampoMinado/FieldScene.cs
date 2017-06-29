using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeccyEngine;
using keyb = SpeccyEngine.SpeccyKeyboard;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CampoMinado
{
    public class FieldScene : SpeccyScene
    {
        FieldCell[,] field = new FieldCell[32, 24];
        static FieldSettings[] fields = new FieldSettings[]
        {
            new FieldSettings{ Foreground = SpeccyColor.Black, Background = SpeccyColor.Yellow, LastScreen = true, MineCount = 64 },
            new FieldSettings{ Foreground = SpeccyColor.Black, Background = SpeccyColor.Cyan, Damsel1X = 6, Damsel1Y = 6, Damsel2X = 25, Damsel2Y = 6, LastScreen = true, MineCount = 64 },
            new FieldSettings{ Foreground = SpeccyColor.Black, Background = SpeccyColor.Green, Damsel1X = 6, Damsel1Y = 12, Damsel2X = 25, Damsel2Y = 12, LastScreen = true, MineCount = 64, ZapperProbability = 2, ZapperBombProbability = 300 },
            new FieldSettings{ Foreground = SpeccyColor.White, Background = SpeccyColor.Magenta, Damsel1X = 6, Damsel1Y = 12, Damsel2X = 20, Damsel2Y = 12, LastScreen = true, MineCount = 78, ZapperProbability = 2, ZapperBombProbability = 300, LiveMineDelay = 500, LiveMineSpeed = 30 },
            new FieldSettings{ Foreground = SpeccyColor.White, Background = SpeccyColor.Red, Damsel1X = 6, Damsel1Y = 8, Damsel2X = 20, Damsel2Y = 8, LastScreen = true, MineCount = 84, ZapperProbability = 2, ZapperBombProbability = 350, LiveMineDelay = 500, LiveMineSpeed = 30 },
            new FieldSettings{ Foreground = SpeccyColor.White, Background = SpeccyColor.Blue, Damsel1X = 6, Damsel1Y = 4, Damsel2X = 20, Damsel2Y = 4, LastScreen = true, MineCount = 96, ZapperProbability = 2, ZapperBombProbability = 400, LiveMineDelay = 400, LiveMineSpeed = 25 },
            new FieldSettings{ Foreground = SpeccyColor.White, Background = SpeccyColor.Black, Damsel1X = 6, Damsel1Y = 6, Damsel2X = 24, Damsel2Y = 6, LastScreen = true, MineCount = 48, LiveMineDelay = 250, LiveMineSpeed = 5 },
            new FieldSettings{ Foreground = SpeccyColor.Black, Background = SpeccyColor.Yellow, Damsel1X = 6, Damsel1Y = 14, Damsel2X = 24, Damsel2Y = 14, LastScreen = true, GatesClosed = true, MineCount = 64, ZapperProbability = 2, ZapperBombProbability = 350 },
            new FieldSettings{ Foreground = SpeccyColor.Black, Background = SpeccyColor.Cyan, LastScreen = false, GatesClosed = true, MineCount = 78 },
        };

        SpeccySprite bg;
        SpeccySprite player;
        SpeccySprite damsel1;
        SpeccySprite damsel2;
        SpeccySprite zapper;
        SpeccySprite liveMine;

        SpeccyLabel tlLabel;
        SpeccyLabel trLabel;
        SpeccyLabel blLabel;
        SpeccyLabel brLabel;
        SpeccyLabel topLabel;
        SpeccyLabel centerLabel;
        SpeccyLabel bottomLabel;

        SpeccyAnimation currentAnimation;

        SpeccyFrame frmMine;
        SpeccyFrame frmMineExplode;

        List<SpeccySprite> allMines = new List<SpeccySprite>();

        FieldSettings currentField;

        int score = 0;
        int currentLevel;
        int adjacent = 0;
        List<coord> playerReplay = new List<coord>();

        Random rnd = new Random();

        int speedPoints = 200;

        int liveMineStep = -1;
        int liveMineCounter = 0;

        bool dead = false;

        List<coord> ensuredPath;
        SpeccyFont fnt;

        SpeccyBeeper beep;

        public FieldScene(int LevelNumber, int CurrentScore = 0) : base()
        {
            beep = new SpeccyBeeper();
            FPS = 20;
            CreateFont();
            currentLevel = LevelNumber + 1;
            score = CurrentScore;
            currentField = fields[LevelNumber].Clone();
            GenerateBackground();
            GenerateDamsels();
            GeneratePlayer();
            GenerateMines();
            GenerateLiveMine();
            GenerateZapper();
            GenerateLabels();
            Intro();
            //CheckMines(player.X, player.Y);
        }

        private void CreateFont()
        {
            fnt =new  SpeccyFont(new Font("ZX Spectrum", 8, GraphicsUnit.Pixel));

            fnt.SetChar('@', new string[] {
                "...@@...",
                "...@@...",
                "..@..@..",
                "@@....@@",
                "@@....@@",
                "..@..@..",
                "...@@...",
                "...@@...",
            });

            fnt.SetChar('%', new string[] {
                ".@.@@.@.",
                ".@.@@.@.",
                "..@..@..",
                "@@.@@.@@",
                "@@.@@.@@",
                "..@..@..",
                ".@.@@.@.",
                "@..@@..@",
            });

            fnt.SetChar('-', new string[] {
                "........",
                "........",
                "........",
                "........",
                "..@@@@..",
                ".@@@@@@.",
                "........",
                "........",
            });

            fnt.SetChar('*', new string[] {
                "........",
                "........",
                "........",
                "........",
                "..@@@@..",
                ".@@@@@@.",
                "@.@..@.@",
                "@.@..@.@",
            });

            fnt.SetChar('_', new string[] {
                "@.@..@.@",
                "@.@..@.@",
                ".@.@@.@.",
                "..@.@...",
                "@.@@@@.@",
                ".@@@@@@.",
                "@......@",
                "........",
            });

            fnt.SetChar('=', new string[] {
                "...@@...",
                ".@@..@@.",
                ".@@..@..",
                "@.@@@@@@",
                "@.....@@",
                ".@@..@..",
                ".@@..@@.",
                "...@@...",
            });

            fnt.SetChar('#', new string[] {
                "....@...",
                "...@....",
                "....@...",
                "@.@@@.@.",
                ".@.@@@.@",
                "...@....",
                "....@...",
                "...@....",
            });

            fnt.SetChar('<', new string[] {
                "...@..@.",
                "..@.@.@.",
                "@..@..@.",
                ".@@@@@..",
                "...@....",
                "..@@@...",
                ".@@@@@..",
                ".@@@@@..",
            });

            fnt.SetChar('>', new string[] {
                ".@..@...",
                ".@.@.@..",
                ".@..@..@",
                "..@@@@@.",
                "....@...",
                "...@@@..",
                "..@@@@@.",
                "..@@@@@.",
            });

            fnt.SetChar('º', new string[] {
                "@@......",
                "..@.....",
                "...@..@.",
                "....@@.@",
                "..@@@@@@",
                "...@....",
                "..@.....",
                "@@......",
            });
        }

        private void GenerateLiveMine()
        {
            if (currentField.LiveMineDelay != 0)
            {
                SpeccyFrame frmLiveMine = new SpeccyFrame(1, 1, new string[] { "*" }, SpeccyColor.Black, SpeccyColor.White);

                liveMine = new SpeccySprite(1, 1, fnt);
                liveMine.AddFrame(frmLiveMine);
                liveMine.AddFrame(frmMineExplode);
                Sprites.Add(liveMine);
                liveMine.Visible = false;
                liveMine.X = 15;
                liveMine.Y = 23;
            }
        }

        private void GenerateZapper()
        {
            if (currentField.ZapperProbability != 0)
            {
                SpeccyFrame frmZap = new SpeccyFrame(1, 1, new string[] { "º" }, currentField.Foreground, currentField.Background);
                zapper = new SpeccySprite(1, 1, fnt);
                zapper.AddFrame(frmZap);
                zapper.Visible = false;
                Sprites.Add(zapper);
            }
        }

        private void Intro()
        {
            tlLabel.Text = "MINAS CERCA 0";
            tlLabel.Flash = false;
            tlLabel.BackColor = SpeccyColor.Black;
            tlLabel.ForeColor = SpeccyColor.White;
            SetScore();
            blLabel.Visible = true;
            blLabel.Text = "NIVEL " + currentLevel;
            blLabel.Alignment = SpeccyLabelAlignment.Center;
            blLabel.BackColor = SpeccyColor.Red;
            blLabel.ForeColor = SpeccyColor.White;
            blLabel.Flash = true;

            brLabel.Visible = true;
            brLabel.Text = "PONIENDO MINAS";
            brLabel.Alignment = SpeccyLabelAlignment.Center;
            brLabel.BackColor = SpeccyColor.White;
            brLabel.ForeColor = SpeccyColor.Red;
            brLabel.Flash = true;

            int frames = 60;
            int frameLen = 2;

            int framesForDamsels = currentField.Damsel1X != 0 || currentField.Damsel2X != 0 ? 60 : 0;

            int framesForField = 24;

            int framesForGates = currentField.GatesClosed ? 60 : 0;

            int framesForMap = currentField.LastScreen ? 0 : 60;

            SpeccyAnimation anim = new SpeccyAnimation((frames + 1) * frameLen + 1 + framesForDamsels + 45 + framesForField + framesForGates + framesForMap + 50);

            player.Visible = false;

            if (damsel1 != null)
                damsel1.Visible = false;

            if (damsel2 != null)
                damsel2.Visible = false;

            tlLabel.Visible = false;
            trLabel.Visible = false;
            blLabel.Visible = false;
            brLabel.Visible = false;

            for (int buc = 0; buc < framesForField; buc++)
            {
                int currentFrame = buc + 1;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = currentFrame,
                    FrameAction = () =>
                    {
                        for(int rbuc = 0; rbuc < 32; rbuc++)
                        {
                            if (field[rbuc, currentFrame - 1] == FieldCell.Wall)
                                bg[rbuc, currentFrame - 1].CurrentChar = '#';
                        }

                        if (currentFrame == 24)
                        {
                            player.Visible = true;
                            tlLabel.Visible = true;
                            trLabel.Visible = true;
                            blLabel.Visible = true;
                            brLabel.Visible = true;
                        }
                    }
                });
            }

            for (int buc = 0; buc < frames; buc++)
            {
                int currentFrame = buc * frameLen + 1 + framesForField;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = currentFrame,
                    FrameAction = () =>
                    {
                        beep.PlayAsync(1500, 2);
                    }
                });
            }

            if (framesForDamsels > 0)
            {
                int currentFrame = frames * frameLen + 3 + framesForField;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = currentFrame,
                    FrameAction = () =>
                    {
                        damsel1.Visible = true;
                        damsel2.Visible = true;
                        blLabel.Visible = false;
                        brLabel.Visible = false;
                        bottomLabel.Visible = true;
                        bottomLabel.Text = "RESCATA DAMISELAS POR PUNTOS!";
                        bottomLabel.Flash = false;
                        bottomLabel.ForeColor = currentField.Background;
                        bottomLabel.BackColor = currentField.Foreground;

                    }
                });

                int step = framesForDamsels / 20;

                for (int buc = 0; buc < framesForDamsels; buc += step)
                {
                    int tcurrentFrame = frames * frameLen + 5 + buc + framesForField;

                    anim.AddKeyFrame(new SpeccyKeyFrame
                    {
                        Frame = tcurrentFrame,
                        FrameAction = () =>
                        {
                            beep.PlayAsync(5000, 10);
                            damsel1.CurrentFrame = damsel1.CurrentFrame == 0 ? 1 : 0;
                            damsel2.CurrentFrame = damsel2.CurrentFrame == 0 ? 1 : 0;

                        }
                    });
                }

                
            }

            if (framesForGates > 0)
            {
                int showFrame = frames * frameLen + 10 + framesForDamsels + framesForField + 20;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = showFrame,
                    FrameAction = () =>
                    {
                        beep.PlayAsync(7500, 100);
                        bottomLabel.Visible = true;
                        bottomLabel.Flash = true;
                        bottomLabel.BackColor = SpeccyColor.White;
                        bottomLabel.ForeColor = SpeccyColor.Black;
                        bottomLabel.Text = "ESCONDETE EN 3 MINAS PARA ABRIR";
                        brLabel.Visible = false;
                        blLabel.Visible = false;
                        centerLabel.Visible = true;
                        centerLabel.Flash = false;
                        centerLabel.BackColor = SpeccyColor.White;
                        centerLabel.ForeColor = SpeccyColor.Blue;
                        centerLabel.Text = "CERRADO";
                        field[15, 1] = FieldCell.Wall;
                        field[16, 1] = FieldCell.Wall;
                        bg[15, 1].CurrentChar = '#';
                        bg[16, 1].CurrentChar = '#';
                    }
                });

                int hideFrame = frames * frameLen + 10 + framesForDamsels + framesForField + framesForGates - 1;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = hideFrame,
                    FrameAction = () =>
                    {
                        beep.PlayAsync(7500, 100);
                        bottomLabel.Visible = false;
                        centerLabel.Visible = false;
                    }
                });
            }

            if (framesForMap > 0)
            {
                int showFrame = frames * frameLen + 10 + framesForDamsels + framesForGates + framesForField + 20;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = showFrame,
                    FrameAction = () =>
                    {
                        centerLabel.Visible = false;
                        bottomLabel.Visible = true;
                        bottomLabel.Flash = true;
                        bottomLabel.BackColor = SpeccyColor.Black;
                        bottomLabel.ForeColor = SpeccyColor.BrightRed;
                        bottomLabel.Text = "TU MAPA NO FUNCIONA!!!";
                        bottomLabel.Flash = true;


                    }
                });

                int nFrame = showFrame + 1;
                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = nFrame,
                    FrameAction = () =>
                    {
                        beep.Play(4000, 100);
                        beep.Play(3800, 100);
                        beep.Play(4000, 100);
                        beep.Play(3800, 100);
                        beep.Play(4000, 100);
                        beep.Play(3800, 100);
                    }
                });

                int hideFrame = frames * frameLen + 10 + framesForDamsels + framesForField + framesForGates + framesForMap - 1;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = hideFrame,
                    FrameAction = () =>
                    {
                        beep.PlayAsync(7500, 100);
                        bottomLabel.Visible = false;
                        centerLabel.Visible = false;
                    }
                });
            }

            int ccurrentFrame = frames * frameLen + 10 + framesForDamsels + framesForField + framesForGates + framesForMap;

            anim.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = ccurrentFrame,
                FrameAction = () =>
                {
                    bottomLabel.Visible = false;
                    brLabel.Visible = false;

                    blLabel.Visible = true;
                    blLabel.Flash = true;
                    blLabel.ForeColor = currentField.Background;
                    blLabel.BackColor = currentField.Foreground;
                    blLabel.Alignment = SpeccyLabelAlignment.Center;
                    blLabel.Text = "COMIENZA!";

                }
            });

            for (int buc = 0; buc < 12; buc += 1)
            {
                int currentFrame = frames * frameLen + 11 + framesForDamsels + buc + framesForField + framesForGates + framesForMap;
                int freq = 4500 + buc * 50;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = currentFrame,
                    FrameAction = () =>
                    {
                        beep.PlayAsync((ushort)freq, 16);
                    }
                });
            }


            int fcurrentFrame = frames * frameLen + 34 + framesForDamsels + framesForField + framesForGates + framesForMap;

            anim.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = fcurrentFrame,
                FrameAction = () =>
                {
                    blLabel.Visible = false;
                    CheckMines(player.X, player.Y);
                    SetScore();
                }
            });

            currentAnimation = anim;
        }

        private void GenerateMines()
        {
            int xStart = rnd.Next(0, 3) + 14;
            int xEnd = rnd.Next(0, 2) + 15;
            coord start = new coord { x = xStart, y = 22 };
            coord end = new coord { x = xEnd, y = 2 };

            coord currentCoord = start;
            
            ensuredPath = new List<coord>();
            ensuredPath.Add(currentCoord);

            direction currentDir = direction.up;

            bool skip = false;

            while (!currentCoord.Equals(end))
            {
                switch (currentDir)
                {
                    case direction.up:
                        currentCoord.y -= 1;
                        break;
                    case direction.left:
                        currentCoord.x -= 1;
                        break;
                    case direction.right:
                        currentCoord.x += 1;
                        break;
                }

                ensuredPath.Add(currentCoord);

                if (currentCoord.y == end.y)
                {
                    if (currentCoord.x < end.x)
                        currentDir = direction.right;
                    else
                        currentDir = direction.left;
                }
                else
                {
                    if (currentDir == direction.left && currentCoord.x == 1 || currentDir == direction.right && currentCoord.x == 30)
                    {
                        currentDir = direction.up;
                        skip = false;
                    }
                    else
                    {
                        if (!skip && rnd.NextDouble() > 0.5f)
                        {
                            skip = true;

                            if (currentDir == direction.left || currentDir == direction.right)
                                currentDir = direction.up;
                            else
                            {
                                if (currentCoord.x == 1 || currentCoord.x == 30)
                                    currentDir = direction.up;
                                else
                                    currentDir = rnd.NextDouble() > 0.499999 ? direction.left : direction.right;
                            }
                        }
                        else
                            skip = false;
                    }
                }
            }
            
            List<coord> minePlaces = new List<coord>();

            for (int buc = 0; buc < currentField.MineCount; buc++)
            {
                bool done = false;

                while (!done)
                {
                    coord c = new coord { x = rnd.Next(1, 31), y = rnd.Next(2, 22) };

                    if (minePlaces.Contains(c) || 
                        ensuredPath.Contains(c) ||
                        (damsel1 != null && damsel1.X == c.x && damsel1.Y == c.y)||
                        (damsel2 != null && damsel2.X == c.x && damsel2.Y == c.y))
                        continue;

                    minePlaces.Add(c);

                    done = true;
                }
            }

            frmMine = new SpeccyFrame(1, 1, new string[] { "-" }, currentField.Foreground, currentField.Background);
            frmMineExplode = new SpeccyFrame(1, 1, new string[] { "_" }, currentField.Foreground, currentField.Background);

            foreach (var coord in minePlaces)
            {
                SpeccySprite mine = new SpeccySprite(1, 1, fnt);
                mine.AddFrame(frmMine);
                mine.AddFrame(frmMineExplode);
                mine.X = coord.x;
                mine.Y = coord.y;
                mine.Visible = false;
                field[coord.x, coord.y] = FieldCell.Mine;
                Sprites.Add(mine);
                allMines.Add(mine);
            }

           
        }

        struct coord
        {
            public int x;
            public int y;
        }

        public enum direction
        {
            up,
            left,
            right
        }

        private void GeneratePlayer()
        {
            SpeccyFrame playerFrame = new SpeccyFrame(1, 1, new string[] { "@" }, 
                currentField.LastScreen ? SpeccyColor.Black : currentField.Foreground, 
                currentField.LastScreen ? SpeccyColor.White : currentField.Background);

            SpeccyFrame playerDamselFrame = new SpeccyFrame(1, 1, new string[] { "%" }, 
                currentField.LastScreen ? SpeccyColor.Black : currentField.Foreground,
                currentField.LastScreen ? SpeccyColor.White : currentField.Background);

            SpeccyFrame playerExplodeFrame = new SpeccyFrame(1, 1, new string[] { "=" },
                currentField.LastScreen ? SpeccyColor.Black : currentField.Foreground,
                currentField.LastScreen ? SpeccyColor.White : currentField.Background);

            player = new SpeccySprite(1, 1, fnt);
            player.AddFrame(playerFrame);
            player.AddFrame(playerDamselFrame);
            player.AddFrame(playerExplodeFrame);
            player.X = 15;
            player.Y = 23;
            Sprites.Add(player);
        }

        private void GenerateLabels()
        {
            topLabel = new SpeccyLabel(32, SpeccyColor.White, SpeccyColor.Blue, fnt);
            topLabel.Flash = true;
            topLabel.Alignment = SpeccyLabelAlignment.Center;
            topLabel.Text = "PONIENDO MINAS";
            topLabel.Visible = false;

            bottomLabel = new SpeccyLabel(32, SpeccyColor.White, SpeccyColor.Black, fnt);
            bottomLabel.Alignment = SpeccyLabelAlignment.Center;
            bottomLabel.Y = 23;
            bottomLabel.Text = "";
            bottomLabel.Visible = false;

            centerLabel = new SpeccyLabel(16, SpeccyColor.White, SpeccyColor.Red, fnt);
            centerLabel.Alignment = SpeccyLabelAlignment.Center;
            centerLabel.Y = 12;
            centerLabel.X = 8;
            centerLabel.Text = "";
            centerLabel.Visible = false;

            tlLabel = new SpeccyLabel(15, SpeccyColor.Black, SpeccyColor.Green, fnt);
            //tlLabel.Visible = false;
            trLabel = new SpeccyLabel(15, SpeccyColor.White, SpeccyColor.Black, fnt);
            trLabel.X = 17;
            //trLabel.Visible = false;
            blLabel = new SpeccyLabel(14, SpeccyColor.White, SpeccyColor.Blue, fnt);
            blLabel.Y = 23;
            blLabel.Visible = false;
            brLabel = new SpeccyLabel(15, SpeccyColor.White, SpeccyColor.Blue, fnt);
            brLabel.Y = 23;
            brLabel.X = 17;
            brLabel.Visible = false;

            tlLabel.Text = "MINAS CERCA 0";
            SetScore();

            Sprites.Add(tlLabel);
            Sprites.Add(trLabel);
            Sprites.Add(blLabel);
            Sprites.Add(brLabel);
            Sprites.Add(topLabel);
            Sprites.Add(bottomLabel);
            Sprites.Add(centerLabel);
        }

        private void SetScore()
        {
            trLabel.Text = "NO." + currentLevel + " PUNT. " + score;
        }

        private void GenerateDamsels()
        {
            SpeccyFrame frmDamselL = new SpeccyFrame(1, 1, new string[] { ">" }, currentField.Foreground, currentField.Background);
            SpeccyFrame frmDamselR = new SpeccyFrame(1, 1, new string[] { "<" }, currentField.Foreground, currentField.Background);

            if (currentField.Damsel1X != 0 && currentField.Damsel1Y != 0)
            {
                
                damsel1 = new SpeccySprite(1, 1, fnt);
                damsel1.AddFrame(frmDamselL);
                damsel1.AddFrame(frmDamselR);
                damsel1.X = currentField.Damsel1X;
                damsel1.Y = currentField.Damsel1Y;
                Sprites.Add(damsel1);
                field[damsel1.X, damsel1.Y] = FieldCell.Damsel;
            }

            if (currentField.Damsel2X != 0 && currentField.Damsel2Y != 0)
            {
                
                damsel2 = new SpeccySprite(1, 1, fnt);
                damsel2.AddFrame(frmDamselL);
                damsel2.AddFrame(frmDamselR);
                damsel2.CurrentFrame = 1;
                damsel2.X = currentField.Damsel2X;
                damsel2.Y = currentField.Damsel2Y;
                Sprites.Add(damsel2);
                field[damsel2.X, damsel2.Y] = FieldCell.Damsel;
            }
        }

        private void GenerateBackground()
        {
            SpeccyFrame bgFrame = new SpeccyFrame(32, 24, null, currentField.Foreground, currentField.Background);

            for (int buc = 0; buc < 32; buc++)
            {
                if (buc == 14 || buc == 15 || buc == 16)
                {
                    if (buc == 14)
                    {
                        field[buc, 1] = FieldCell.Wall;
                        //bgFrame[buc, 1].Char = '#';
                    }
                }
                else
                {
                    //bgFrame[buc, 1].Char = '#';
                    //bgFrame[buc, 22].Char = '#';

                    field[buc, 1] = FieldCell.Wall;
                    field[buc, 22] = FieldCell.Wall;
                }
            }

            for (int buc = 1; buc < 23; buc++)
            {
                //bgFrame[0, buc].Char = '#';
                //bgFrame[31, buc].Char = '#';

                field[0, buc] = FieldCell.Wall;
                field[31, buc] = FieldCell.Wall;
            }

            bg = new SpeccySprite(32, 24, fnt);
            bg.X = 0;
            bg.Y = 0;
            bg.TranspatentChar = '\0';
            bg.AddFrame(bgFrame);
            Sprites.Add(bg);
        }
        
        private void Explode(int X, int Y)
        {
            tlLabel.Text = "BOOOM!!!!";

            currentAnimation = new SpeccyAnimation(60);

            int currentPlayer = player.CurrentFrame;
            var mine = allMines.Where(m => m.X == X && m.Y == Y).FirstOrDefault();
            player.Visible = false;
            mine.Visible = true;

            if (liveMine != null)
            {
                Sprites.Remove(liveMine);
                liveMine = null;
                currentField.LiveMineDelay = 0;
            }

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 3,
                FrameAction = () =>
                {
                    mine.CurrentFrame = 1;
                    beep.PlayAsync(100, 10);
                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 6,
                FrameAction = () =>
                {
                    mine.CurrentFrame = 0;
                    beep.PlayAsync(100, 10);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 9,
                FrameAction = () =>
                {
                    mine.CurrentFrame = 1;
                    beep.PlayAsync(110, 10);

                }
            });
            
            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 12,
                FrameAction = () =>
                {
                    mine.CurrentFrame = 0;
                    beep.PlayAsync(110, 10);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 15,
                FrameAction = () =>
                {
                    mine.CurrentFrame = 1;
                    beep.PlayAsync(100, 10);

                }
            });
            
            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 18,
                FrameAction = () =>
                {
                    mine.CurrentFrame = 0;
                    beep.PlayAsync(100, 10);
                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 21,
                FrameAction = () =>
                {
                    mine.CurrentFrame = 1;
                    beep.PlayAsync(110, 10);

                }
            });

            ShowMines(currentAnimation, 21);

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 60,
                FrameAction = () =>
                {
                    Replay();

                }
            });
        }

        private void ShowMines(SpeccyAnimation MixTo, int BaseFrame)
        {
            for (int buc = 2; buc < 22; buc += 1)
            {
                int frame = (buc - 1) + BaseFrame;
                int row = buc;

                MixTo.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = frame,
                    FrameAction = () =>
                    {
                        var minesToShow = allMines.Where(m => m.Y == row);
                        foreach (var mine in minesToShow)
                        {
                            mine.Visible = true;
                            mine.CurrentFrame = 0;
                        }

                    }
                });
            }
        }

        private void Replay()
        {
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 24; y++)
                    bg[x, y].BackColor = currentField.Background;
            }

            player.CurrentFrame = 0;
            player.X = 15;
            player.Y = 23;
            player.Visible = true;

            blLabel.Visible = true;
            blLabel.Text = "REPETICION";
            blLabel.Flash = true;
            blLabel.ForeColor = SpeccyColor.White;
            blLabel.BackColor = SpeccyColor.Black;
            blLabel.Alignment = SpeccyLabelAlignment.Center;

            var anim = new SpeccyAnimation((playerReplay.Count + 1) * 5);


            int freq = 220;
            int maxFreq = 1500;

            float audioStep = (maxFreq - freq) / (float)playerReplay.Count;

            for (int buc = 0; buc < playerReplay.Count; buc++)
            {
                int frame = (buc + 1) * 5;
                int step = buc;
                int currentFreq = (int)(audioStep * buc) + freq;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = frame,
                    FrameAction = () => 
                    {
                        bg[player.X, player.Y].BackColor = SpeccyColor.White;
                        var coord = playerReplay[step];
                        player.X = coord.x;
                        player.Y = coord.y;
                        beep.PlayAsync((ushort)currentFreq, 100);

                        if (player.Y == 0)
                        {
                            Win();
                        }
                        else
                        if (field[coord.x, coord.y] != FieldCell.Empty || step == playerReplay.Count - 1)
                        {
                            var mine = allMines.Where(m => m.X == coord.x && m.Y == coord.y).FirstOrDefault();

                            if (mine != null)
                                mine.Visible = false;

                            player.CurrentFrame = 2;
                            
                            GameOver();
                        }
                        
                    }
                });
            }

            currentAnimation = anim;
        }

        private void Win()
        {

            int freq = 2500;
            int maxFreq = 5000;
            int frames = 5;

            float audioStep = (maxFreq - freq) / (float)frames;

            int frameLen = 1;

            var anim = new SpeccyAnimation(frames * frameLen + 40);

            for (int buc = 0; buc < frames; buc++)
            {
                int frame = buc * frameLen + 1;
                int currentFreq = (int)(audioStep * buc) + freq;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = frame,
                    FrameAction = () =>
                    {
                        beep.PlayAsync((ushort)currentFreq, frameLen * 50);

                    }
                });

            }

            anim.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = frames * frameLen + 1,
                FrameAction = () =>
                {
                    tlLabel.Visible = false;
                    trLabel.Visible = false;
                    topLabel.Visible = false;
                    blLabel.Visible = false;
                    brLabel.Visible = false;
                    bottomLabel.Visible = false;
                }
            });

            anim.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = frames * frameLen + 30,
                FrameAction = () =>
                {
                    FinishData = new EndLevelInfo { NextScreen = currentLevel, Score = score };
                    Finished = true;
                }
            });

            currentAnimation = anim;
        }

        private void GameOver()
        {
            tlLabel.Visible = false;
            trLabel.Visible = false;
            blLabel.Visible = false;
            brLabel.Visible = false;
            topLabel.Visible = true;
            topLabel.ForeColor = SpeccyColor.White;
            topLabel.BackColor = SpeccyColor.Black;
            topLabel.Flash = false;
            topLabel.Visible = true;
            topLabel.Alignment = SpeccyLabelAlignment.Center;
            topLabel.Text = "REINTENTAR?  NIVEL " + currentLevel + " PUNTOS " + score;

            bottomLabel.Text = "MAXIMOS PUNTOS: 10000 POR GUSMAN";
            bottomLabel.Visible = true;
            bottomLabel.Flash = false;

            centerLabel.Text = "GAME OVER";
            centerLabel.ForeColor = SpeccyColor.White;
            centerLabel.BackColor = SpeccyColor.Red;
            centerLabel.Flash = true;
            centerLabel.Visible = true;

            int freq = 1000;
            int maxFreq = 2500;
            int frames = 5;

            float audioStep = (maxFreq - freq) / (float)frames;

            int frameLen = 1;

            SpeccyAnimation anim = new SpeccyAnimation(1);

            anim.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 1,
                FrameAction = () =>
                {
                    beep.Play(100, 15);
                    beep.Play(110, 15);
                    beep.Play(100, 15);
                    beep.Play(115, 15);
                    beep.Play(90, 15);
                    beep.Play(115, 15);
                    beep.Play(99, 15);
                    beep.Play(108, 15);
                }

            });

            currentAnimation = anim;
            dead = true;
        }



        private bool CheckMines(int X, int Y)
        {
            if (field[X, Y] == FieldCell.Mine || field[X, Y] == FieldCell.Wall)
                return false;

            adjacent = 0;

            if (X > 0 && field[X - 1, Y] != FieldCell.Empty)
                adjacent++;

            if (X < 31 && field[X + 1, Y] != FieldCell.Empty)
                adjacent++;

            if (Y > 0 && field[X, Y - 1] != FieldCell.Empty)
                adjacent++;

            if (Y < 23 && field[X, Y + 1] != FieldCell.Empty)
                adjacent++;

            if (adjacent == 0)
            {
                tlLabel.ForeColor = SpeccyColor.Black;
                tlLabel.BackColor = SpeccyColor.Green;
                tlLabel.Text = "MINAS CERCA 0";
                beep.PlayAsync(110, 10);
            }
            else if (adjacent == 1)
            {
                tlLabel.ForeColor = SpeccyColor.White;
                tlLabel.BackColor = SpeccyColor.Magenta;
                tlLabel.Text = "MINAS CERCA 1";
                beep.PlayAsync(330, 50);
            }
            else if (adjacent == 2)
            {
                tlLabel.ForeColor = SpeccyColor.White;
                tlLabel.BackColor = SpeccyColor.Red;
                tlLabel.Text = "MINAS CERCA 2";
                beep.PlayAsync(440, 50);
            }
            else if (adjacent == 3)
            {
                tlLabel.ForeColor = SpeccyColor.White;
                tlLabel.BackColor = SpeccyColor.Blue;
                tlLabel.Text = "MINAS CERCA 3";
                beep.PlayAsync(550, 50);
            }

            return true;
        }

        uint frame = 0;

        public override void Update()
        {

            

            if (currentAnimation != null)
            {
                if (currentAnimation.Finished)
                    currentAnimation = null;
                else
                {
                    currentAnimation.Play();
                    return;
                }
            }

            if (dead)
            {
                if (keyb.AnyPressed())
                {
                    Finished = true;
                    FinishData = new EndLevelInfo();
                    return;
                }

                return;
            }

            if (currentField.ZapperProbability != 0)
            {
                int val = rnd.Next(0, 1000);

                if (val < currentField.ZapperProbability)
                {
                    ShowZapper();
                    return;
                }
            }

            if (currentField.LiveMineDelay > 1)
            {
                currentField.LiveMineDelay--;
            }
            else if (currentField.LiveMineDelay == 1)
            {
                if (liveMine.Visible)
                {
                    liveMineCounter++;
                    if (liveMineCounter >= currentField.LiveMineSpeed)
                    {
                        liveMineStep++;
                        liveMineCounter = 0;
                        var pos = playerReplay[liveMineStep];
                        liveMine.X = pos.x;
                        liveMine.Y = pos.y;
                        beep.PlayAsync(7500, 5);
                    }
                }
                else
                    liveMine.Visible = true;

                if (liveMine.X == player.X && liveMine.Y == player.Y)
                {
                    ExplodeLiveMine();
                    return;
                }
            }

            if (speedPoints > 50)
            {
                frame++;

                if (frame % 10 == 0)
                    speedPoints -= 1;
            }

            if (damsel1 != null)
            {
                if (rnd.Next(0, 101) > 98)
                {
                    if (damsel1.CurrentFrame == 0)
                        damsel1.CurrentFrame = 1;
                    else
                        damsel1.CurrentFrame = 0;
                }
            }

            if (damsel2 != null)
            {
                if (rnd.Next(0, 101) > 98)
                {
                    if (damsel2.CurrentFrame == 0)
                        damsel2.CurrentFrame = 1;
                    else
                        damsel2.CurrentFrame = 0;
                }
            }

            int newCoordX = -1;
            int newCoordY = -1;

            if (keyb.IsClicked(Keys.Up))
            {
                if (player.Y > 0)
                {
                    newCoordX = player.X;
                    newCoordY = player.Y - 1;
                }
            }
            else if (keyb.IsClicked(Keys.Down))
            {
                if (player.Y < 23)
                {
                    newCoordX = player.X;
                    newCoordY = player.Y + 1;
                }
            }
            else if (keyb.IsClicked(Keys.Left))
            {
                if (player.X > 0)
                {
                    newCoordX = player.X - 1;
                    newCoordY = player.Y;
                }
            }
            else if (keyb.IsClicked(Keys.Right))
            {
                if (player.X < 31)
                {
                    newCoordX = player.X + 1;
                    newCoordY = player.Y;
                }
            }

            if (newCoordX != -1)
            {
                if (!CheckWalls(newCoordX, newCoordY))
                    return;

                if (!CheckMines(newCoordX, newCoordY))
                {
                    playerReplay.Add(new coord { x = newCoordX, y = newCoordY });
                    Explode(newCoordX, newCoordY);
                    return;
                }

                if(currentField.LastScreen)
                    bg.Frame[player.X, player.Y].BackColor = SpeccyColor.White;

                player.X = newCoordX;
                player.Y = newCoordY;

                CheckDamsels(newCoordX, newCoordY);
                SetScore();

                playerReplay.Add(new coord { x = newCoordX, y = newCoordY });

                if (adjacent == 3 && currentField.GatesClosed)
                    OpenGate();

                if (newCoordY == 0)
                    GotHome();
            }
        }

        private void OpenGate()
        {
            var anim = new SpeccyAnimation(30);

            anim.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 1,
                FrameAction = () =>
                {
                    beep.PlayAsync(7500, 100);
                    
                    centerLabel.Visible = true;
                    centerLabel.Flash = false;
                    centerLabel.BackColor = SpeccyColor.White;
                    centerLabel.ForeColor = SpeccyColor.Blue;
                    centerLabel.Text = "ABIERTO";
                    field[15, 1] = FieldCell.Empty;
                    field[16, 1] = FieldCell.Empty;
                    bg[15, 1].CurrentChar = ' ';
                    bg[16, 1].CurrentChar = ' ';
                }
            });
            
            anim.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 29,
                FrameAction = () =>
                {
                    beep.PlayAsync(10000, 100);
                    centerLabel.Visible = false;
                }
            });

            currentAnimation = anim;
        }

        private void ExplodeLiveMine()
        {
            tlLabel.Text = "BOOOM!!!!";

            currentAnimation = new SpeccyAnimation(60);

            int currentPlayer = player.CurrentFrame;
            player.Visible = false;
            currentField.LiveMineDelay = 0;

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 3,
                FrameAction = () =>
                {
                    liveMine.CurrentFrame = 1;
                    beep.PlayAsync(100, 10);
                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 6,
                FrameAction = () =>
                {
                    liveMine.CurrentFrame = 0;
                    beep.PlayAsync(100, 10);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 9,
                FrameAction = () =>
                {
                    liveMine.CurrentFrame = 1;
                    beep.PlayAsync(110, 10);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 12,
                FrameAction = () =>
                {
                    liveMine.CurrentFrame = 0;
                    beep.PlayAsync(110, 10);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 15,
                FrameAction = () =>
                {
                    liveMine.CurrentFrame = 1;
                    beep.PlayAsync(100, 10);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 18,
                FrameAction = () =>
                {
                    liveMine.CurrentFrame = 0;
                    beep.PlayAsync(100, 10);
                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 21,
                FrameAction = () =>
                {
                    liveMine.CurrentFrame = 1;
                    beep.PlayAsync(110, 10);

                }
            });

            ShowMines(currentAnimation, 21);

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 60,
                FrameAction = () =>
                {
                    liveMine.Visible = false;
                    player.Visible = true;
                    Replay();

                }
            });
        }

        private void ShowZapper()
        {
            int y = rnd.Next(0, 101) > 49 ? player.Y + 2 : player.Y - 2;

            if (y < 3)
                y += 4;

            if (y > 19)
            {
                if (player.Y > 19)
                    y = 19;
                else
                    y -= 4;
            }
            zapper.Visible = true;
            zapper.X = 1;
            zapper.Y = y;

            beep.PlayAsync(6500, 5);

            int frameLen = 3;

            SpeccyAnimation anim = new SpeccyAnimation(32 * frameLen);

            brLabel.Visible = true;
            brLabel.Flash = true;
            brLabel.Text = "MAS MINAS!";
            brLabel.ForeColor = currentField.Foreground;
            brLabel.BackColor = currentField.Background;

            for (int x = 2; x < 31; x++)
            {
                int curX = x;
                int curY = y;
                int curFrame = (x - 1) * frameLen;

                anim.AddKeyFrame(new SpeccyKeyFrame
                {
                    Frame = curFrame,
                    FrameAction = () =>
                    {
                        beep.PlayAsync(6500, 5);
                        zapper.X = curX;
                        zapper.Y = curY;

                        int mineX = curX - 1;

                        for (int of = -1; of < 2; of += 1)
                        {
                            int mineY = curY + of;

                            if (rnd.Next(0, 1000) < currentField.ZapperBombProbability)
                            {
                            
                                if (mineX != player.X && mineY != player.Y && bg[mineX,mineY].BackColor != SpeccyColor.White && 
                                (field[mineX, mineY] == FieldCell.Mine || field[mineX, mineY] == FieldCell.Empty) && 
                                !ensuredPath.Contains(new coord { x = mineX, y = mineY }))
                                {
                                    var existing = allMines.Where(m => m.X == mineX && m.Y == mineY).FirstOrDefault();

                                    if (existing == null)
                                    {
                                        SpeccySprite mine = new SpeccySprite(1, 1, fnt);
                                        mine.AddFrame(frmMine);
                                        mine.AddFrame(frmMineExplode);
                                        mine.X = mineX;
                                        mine.Y = mineY;
                                        mine.Visible = true;
                                        field[mineX, mineY] = FieldCell.Mine;
                                        Sprites.Add(mine);
                                        allMines.Add(mine);

                                    }
                                }
                            }
                            else
                            {
                                var existing = allMines.Where(m => m.X == mineX && m.Y == mineY).FirstOrDefault();

                                if (existing != null)
                                {
                                    Sprites.Remove(existing);
                                    field[mineX, mineY] = FieldCell.Empty;
                                }
                            }
                        }
                    }
                });

            }

            anim.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 30 * frameLen,
                FrameAction = () => 
                {
                    beep.PlayAsync(6500, 5);
                    zapper.Visible = false;
                    brLabel.Visible = false;
                    Sprites.Remove(zapper);
                    Sprites.Add(zapper);
                    CheckMines(player.X, player.Y);
                }
            });

            currentAnimation = anim;
        }

        private bool CheckWalls(int X, int Y)
        {
            return field[X, Y] != FieldCell.Wall;
        }

        private void GotHome()
        {
            score += speedPoints;
            SetScore();

            int freq = 2500;
            int maxFreq = 5000;
            int frames = 5;

            float audioStep = (maxFreq - freq) / (float)frames;

            int frameLen = 1;

            tlLabel.Visible = false;
            trLabel.Visible = false;
            brLabel.Visible = false;
            blLabel.Visible = false;

            topLabel.Visible = true;
            topLabel.Alignment = SpeccyLabelAlignment.Center;
            topLabel.ForeColor = SpeccyColor.White;
            topLabel.BackColor = SpeccyColor.Blue;
            topLabel.Text = "NIVEL " + currentLevel + " PUNTOS VEL. " + speedPoints;
            topLabel.Flash = false;

            if (liveMine != null && liveMine.Visible)
                liveMine.Visible = false;
            
            SpeccyAnimation anim = new SpeccyAnimation((frames + 1) * frameLen + 22);

            anim.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 1,
                FrameAction = () =>
                {
                    beep.Play((ushort)7500, 25);
                    beep.Play((ushort)7750, 25);
                    beep.Play((ushort)8000, 25);
                    beep.Play((ushort)8250, 25);
                    beep.Play((ushort)8500, 25);

                }
            });
            
            ShowMines(anim, frames * frameLen + 1);

            int tframe = frames * frameLen + 23;

            anim.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = tframe,
                FrameAction = () =>
                {
                    Replay();

                }
            });

            currentAnimation = anim;
        }

        private void CheckDamsels(int X, int Y)
        {
            if (field[X, Y] == FieldCell.Damsel)
            {
                CreatePickDamselAnimation();
                score += 100;
                field[X, Y] = FieldCell.Empty;
                if (damsel1.X == X && damsel1.Y == Y)
                    damsel1.Visible = false;
                else
                    damsel2.Visible = false;
            }
        }

        private void CreatePickDamselAnimation()
        {
            currentAnimation = new SpeccyAnimation(30);

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 3,
                FrameAction = () =>
                {
                    player.CurrentFrame = 0;
                    beep.PlayAsync(440, 50);
                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 6,
                FrameAction = () =>
                {
                    beep.PlayAsync(385, 50);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 9,
                FrameAction = () =>
                {
                    player.CurrentFrame = 1;
                    beep.PlayAsync(330, 50);

                }
            });
            
            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 12,
                FrameAction = () =>
                {
                    player.CurrentFrame = 0;
                    beep.PlayAsync(440, 50);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 15,
                FrameAction = () =>
                {
                    beep.PlayAsync(385, 50);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 18,
                FrameAction = () =>
                {
                    player.CurrentFrame = 1;
                    beep.PlayAsync(330, 50);

                }
            });
            
            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 21,
                FrameAction = () =>
                {
                    player.CurrentFrame = 0;
                    beep.PlayAsync(440, 50);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 24,
                FrameAction = () =>
                {
                    beep.PlayAsync(385, 50);

                }
            });

            currentAnimation.AddKeyFrame(new SpeccyKeyFrame
            {
                Frame = 27,
                FrameAction = () =>
                {
                    player.CurrentFrame = 1;
                    beep.PlayAsync(330, 50);

                }
            });

        }

        public override void Dispose()
        {
        }

        public enum FieldCell
        {
            Empty,
            Player,
            Mine,
            Wall,
            VisibleMine,
            Miner,
            WalkingMine,
            Damsel
        }

        class FieldSettings
        {
            public SpeccyColor Foreground { get; set; }
            public SpeccyColor Background { get; set; }
            public bool GatesClosed { get; set; }
            public int Damsel1X { get; set; }
            public int Damsel1Y { get; set; }
            public int Damsel2X { get; set; }
            public int Damsel2Y { get; set; }
            public int ZapperProbability { get; set; }
            public int ZapperBombProbability { get; set; }
            public int LiveMineSpeed { get; set; }
            public int LiveMineDelay { get; set; }
            public int MineCount { get; set; }
            public bool LastScreen { get; set; }

            public FieldSettings Clone()
            {
                return this.MemberwiseClone() as FieldSettings;
            }
        }
    }

    public class EndLevelInfo
    {
        public int NextScreen = 0;
        public int Score = 0;
    }
}
