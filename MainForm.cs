// Code heavily inspired from my own, teeny bit old, different programming language, code
// View at https://github.com/raf13lol/PizzaTowerConnected/blob/main/objects/obj_minesweeper/Create_0.gml

using System;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Web;

namespace FormsSweeper
{
    public partial class MainForm : Form
    {
        // grid
        public int[] grid;
        public NButton[] buttonGrid;
        public int gridWidth;
        public int gridHeight;
        public int bombsToMake;
        public List<NButton> tilesToRefreshWhenDead = new List<NButton>();

        public int squaresClicked;
        public int secondsPassed;
        public bool clickedYet;
        public bool madeBefore = false;

        public MinesweeperGameEndType gameState;

        // ui stuff
        public int bombsLeftNumberForDrawing;
        public Image faceImage;
        public bool baseFace = true;

        // ui
        PictureBox minesHundreds;
        PictureBox minesTens;
        PictureBox minesOnes;

        PictureBox secondsHundreds;
        PictureBox secondsTens;
        PictureBox secondsOnes;
        System.Windows.Forms.Timer timer;

        NButton face;

        // cmd args
        int cmdW = 9;
        int cmdH = 9;
        int cmdB = 10;

        public MainForm(string[] args)
        {
            // bad code but i just want to get this done with
            foreach (string arg in args)
            {
                if (arg.StartsWith("-w="))
                {
                    string numS = arg.Substring(3);
                    int num = 9;
                    try
                    {
                        num = Int32.Parse(numS);
                    }
                    catch 
                    {
                        MessageBox.Show("Error occurred whilst parsing the '-w=' argument, are you sure it's correct?");
                        num = 9;
                    }
                    if (num <= 0)
                    {
                        MessageBox.Show("Width can not be 0 or less.");
                        num = 9;
                    }
                    cmdW = num;
                }
                if (arg.StartsWith("-h="))
                {
                    string numS = arg.Substring(3);
                    int num = 9;
                    try
                    {
                        num = Int32.Parse(numS);
                    }
                    catch
                    {
                        MessageBox.Show("Error occurred whilst parsing the '-h=' argument, are you sure it's correct?");
                        num = 9;
                    }
                    if (num <= 0)
                    {
                        MessageBox.Show("Height can not be 0 or less.");
                        num = 9;
                    }
                    cmdH = num;
                }
                if (arg.StartsWith("-b="))
                {
                    string numS = arg.Substring(3);
                    int num = 10;
                    try
                    {
                        num = Int32.Parse(numS);
                    }
                    catch
                    {
                        MessageBox.Show("Error occurred whilst parsing the '-b=' argument, are you sure it's correct?");
                        num = 10;
                    }
                    if (num < 0)
                    {
                        MessageBox.Show("Bomb count can not be less than 0.");
                        num = 10;
                    }
                    cmdB = num;
                }
            }
            InitializeComponent();
            init();
        }

        public void init()
        {
            grid = Array.Empty<int>();

            makeGrid(cmdW, cmdH, !madeBefore);
            bombsToMake = cmdB;

            randomSeed = DateTime.UtcNow.Ticks;

            squaresClicked = 0;
            secondsPassed = 0;

            bombsLeftNumberForDrawing = bombsToMake;
            faceImage = Properties.Resources.face;
            baseFace = true;

            clickedYet = false;
            
            gameState = MinesweeperGameEndType.NONE;

            if (!madeBefore)
            {
                createButtonGrid();

                MouseDown += mouseDownForm;
                MouseUp += mouseUpForm;
                // What hte fuck?
                foreach (Control con in Controls)
                {
                    if (con.Tag != null && ((TagBullshit)con.Tag!).pos[0] == -20)
                        continue;
                    con.MouseDown += mouseDownForm;
                    con.MouseUp += mouseUpForm;
                }
            }
            else
            {
                refreshGrid();
                mouseUpForm(null, new EventArgs());
            }
            refreshMineCount();
            madeBefore = true;
        }

        public double randomSeed;

        public int generateRandomNumber(int min, int max)
        {
            // https://lib.haxe.org/p/extension-randomize/1.0.1/files/extension/randomize/Randomize.hx 
            // Thank you :)
            randomSeed = ((randomSeed * 9401d + 49297d)) % 233280d;
            if (randomSeed < 0d)
                randomSeed *= -1d;
            double random = randomSeed / 233280d;

            int returnVal = (int)Math.Floor(random * (max - min + 1)) + min;
            return returnVal;
        }

        #region Grid making/utils and bombs making
        public int getGridIndex(int x, int y)
        {
            return x + y * gridWidth;
        }
        public void makeGrid(int w, int h, bool refreshImages = true)
        {
            grid = new int[w * h];
            gridWidth = w;
            gridHeight = h;
            for (int i = 0; i < grid.Length; i++)
            {
                grid[i] = (int)MinesweeperTileState.NOT_CLICKED;
            }
            
            // more ui here
            if (refreshImages)
            {
                // pixel measurements
                ClientSize = new Size(w * 32 + 40, h * 32 + 126);
                // white lines on the side
                PictureBox boxLine1 = new PictureBox();
                boxLine1.BackColor = Color.White;
                boxLine1.Location = new Point(0, 0);
                boxLine1.Size = new Size(ClientSize.Width, 6);

                PictureBox boxLine2 = new PictureBox();
                boxLine2.BackColor = Color.White;
                boxLine2.Location = new Point(0, 0);
                boxLine2.Size = new Size(6, ClientSize.Height);

                Controls.Add(boxLine1);
                Controls.Add(boxLine2);

                // tile grid outline
                makeRectForMine(18, 102, w * 31 + 12, h * 31 + 12, 6, Properties.Resources.cornertilegrid);
                // info box outline
                makeRectForMine(18, 16, w * 31 + 12, 74, 4, Properties.Resources.cornerinfo);

                // mines left box outline
                makeRectForMine(32, 28, 82, 50, 2, null);

                int secondsX = (18 + w * 31 + 8) - 92;
                // seconds right box outline
                makeRectForMine(secondsX, 28, 82, 50, 2, null);

                // mines count
                minesHundreds = new PictureBox();
                minesTens = new PictureBox();
                minesOnes = new PictureBox();

                Controls.Add(minesHundreds);
                Controls.Add(minesTens);
                Controls.Add(minesOnes);

                minesHundreds.Location = new Point(34, 30);
                minesHundreds.Image = Properties.Resources.num0;
                minesHundreds.Size = new Size(26, 46);

                minesTens.Location = new Point(60, 30);
                minesTens.Image = Properties.Resources.num1;
                minesTens.Size = new Size(26, 46);

                minesOnes.Location = new Point(86, 30);
                minesOnes.Image = Properties.Resources.num2;
                minesOnes.Size = new Size(26, 46);


                // seconds count
                secondsHundreds = new PictureBox();
                secondsTens = new PictureBox();
                secondsOnes = new PictureBox();

                Controls.Add(secondsHundreds);
                Controls.Add(secondsTens);
                Controls.Add(secondsOnes);

                secondsHundreds.Location = new Point(secondsX + 2, 30);
                secondsHundreds.Image = Properties.Resources.num0;
                secondsHundreds.Size = new Size(26, 46);

                secondsTens.Location = new Point(secondsX + 28, 30);
                secondsTens.Image = Properties.Resources.num0;
                secondsTens.Size = new Size(26, 46);

                secondsOnes.Location = new Point(secondsX + 54, 30);
                secondsOnes.Image = Properties.Resources.num0;
                secondsOnes.Size = new Size(26, 46);

                timer = new System.Windows.Forms.Timer();
                timer.Interval = 1000;
                timer.Tick += new EventHandler(secondPassed);

                // face outline
                makeRectForMine(ClientSize.Width / 2 - 25, 28, 50, 52, 2, null, true);

                face = new NButton();
                face.Image = Properties.Resources.face;
                face.Size = new Size(49, 49);
                face.Location = new Point(ClientSize.Width / 2 - 24, 29);
                face.FlatStyle = FlatStyle.Flat;
                face.FlatAppearance.MouseOverBackColor = Color.Transparent;
                face.FlatAppearance.BorderSize = 0;
                face.Tag = new TagBullshit([-20]);

                face.MouseDown += facePressed;
                face.MouseUp += faceMouseUp;

                Controls.Add(face);
            }
        }

        // generate bombs on click to prevent insta death
        // unless they want too many bombs
        public void makeBombs(int clickX, int clickY)
        {
            int createdBombs = 0;
            bool willDie = bombsToMake >= gridWidth * gridHeight;
            int boomBoomsToMake = Math.Min(bombsToMake, gridWidth * gridHeight);

            while (true)
            {
                if (createdBombs >= boomBoomsToMake)
                    break;
                int x = generateRandomNumber(0, gridWidth - 1);
                int y = generateRandomNumber(0, gridHeight - 1);

                int index = getGridIndex(x, y);

                if (((x != clickX || y != clickY) || willDie) && !gridHasState(grid[index], MinesweeperTileState.BOMB))
                {
                    grid[index] |= (int)MinesweeperTileState.BOMB;
                    tilesToRefreshWhenDead.Add(buttonGrid[index]);
                    createdBombs++;
                }
            }
        }

        public bool gridHasState(int gridNumber, MinesweeperTileState tileState)
        {
            return (gridNumber & (int)tileState) > 0;
        }

        public int gridRemoveState(int gridNumber, MinesweeperTileState tileState)
        {
            // do an OR so it does absouluttely have it
            return (gridNumber |= (int)tileState) ^ (int)tileState;
        }

        #endregion

        #region grid interaction

        public void clickTileCheck(int x, int y)
        {
            if (!clickedYet)
            {
                clickedYet = true;
                makeBombs(x, y);
                timer.Start();
            }

            int gridIndex = getGridIndex(x, y);
            int gridNumber = grid[gridIndex];

            // Hold on
            if (gridHasState(gridNumber, MinesweeperTileState.CLICKED)
            || gridHasState(gridNumber, MinesweeperTileState.FLAGGED))
                return;

            if (!gridHasState(gridNumber, MinesweeperTileState.BOMB))
            {
                clickTile(x, y);

                // CONGRATITIONS!
                if ((gridWidth * gridHeight) - squaresClicked == bombsToMake)
                {
                    gameState = MinesweeperGameEndType.GAMEWIN;
                    faceImage = Properties.Resources.facewin;
                    baseFace = false;
                    foreach (NButton bombOrFlagTile in tilesToRefreshWhenDead)
                    {
                        TagBullshit tag = (TagBullshit)bombOrFlagTile.Tag!;
                        refreshGridTile(tag.pos[0], tag.pos[1]);
                    }
                    timer.Stop();
                    bombsLeftNumberForDrawing = 0;
                    refreshMineCount();
                }

                return;
            }

            // GAMEOVER HERE
            grid[gridIndex] = (int)MinesweeperTileState.BOMB_CLICKED;
            gameState = MinesweeperGameEndType.GAMEOVER;
            faceImage = Properties.Resources.facelose;
            baseFace = false;
            // optimzation
            foreach (NButton bombOrFlagTile in tilesToRefreshWhenDead)
            {
                TagBullshit tag = (TagBullshit)bombOrFlagTile.Tag!;
                refreshGridTile(tag.pos[0], tag.pos[1]);
            }
            timer.Stop();
        }

        public List<int[]>? clickTile(int x, int y, bool looping = false)
        {
            int gridIndex = getGridIndex(x, y);
            int gridNumber = grid[gridIndex];

            // loop
            if (gridHasState(gridNumber, MinesweeperTileState.CLICKED)
            || gridHasState(gridNumber, MinesweeperTileState.FLAGGED))
                return new List<int[]>();

            int numberOfBombsAround = 0;

            grid[gridIndex] ^= (int)MinesweeperTileState.NOT_CLICKED;
            grid[gridIndex] |= (int)MinesweeperTileState.CLICKED;
            squaresClicked++;

            // Firstly. Die, this code looks bad
            // "i hate this so much" original comment
            for (int tileCheckAroundID = 0; tileCheckAroundID < 8; tileCheckAroundID++)
            {
                int newGridX = x;
                int newGridY = y;
                switch (tileCheckAroundID)
                {
                    case 7: newGridY--; break;             // north
                    case 3: newGridY++; break;             // south

                    case 1: newGridX--; break;             // west
                    case 5: newGridX++; break;             // east

                    case 0: newGridX--; newGridY--; break; // northwest
                    case 6: newGridX++; newGridY--; break; // northeast

                    case 2: newGridX--; newGridY++; break; // southwest
                    case 4: newGridX++; newGridY++; break; // southeast
                }

                if (newGridX < 0 || newGridX >= gridWidth
                || newGridY < 0 || newGridY >= gridHeight)
                    continue; // out of grid

                // if has bomb then there are bombs around
                if (gridHasState(grid[getGridIndex(newGridX, newGridY)], MinesweeperTileState.BOMB))
                    numberOfBombsAround++;
            }

            // there!
            grid[gridIndex] |= numberOfBombsAround;

            refreshGridTile(x, y);

            // Wish i could repeat it but whatever blehhhh
            if (numberOfBombsAround > 0)
                return new List<int[]>();

            List<int[]> positionsToClick = new List<int[]>();

            for (int tileCheckAroundID = 0; tileCheckAroundID < 8; tileCheckAroundID++)
            {
                int newGridX = x;
                int newGridY = y;
                switch (tileCheckAroundID)
                {
                    case 7: newGridY--; break;             // north
                    case 3: newGridY++; break;             // south

                    case 1: newGridX--; break;             // west
                    case 5: newGridX++; break;             // east

                    case 0: newGridX--; newGridY--; break; // northwest
                    case 6: newGridX++; newGridY--; break; // northeast

                    case 2: newGridX--; newGridY++; break; // southwest
                    case 4: newGridX++; newGridY++; break; // southeast
                }

                if (newGridX < 0 || newGridX >= gridWidth
                || newGridY < 0 || newGridY >= gridHeight)
                    continue; // out of grid

                if (gridHasState(grid[getGridIndex(newGridX, newGridY)], MinesweeperTileState.CLICKED))
                    continue;

                // ...
                // this cannot be a bomb for obvious reasons
                positionsToClick.Add([newGridX, newGridY]);
            }

            if (looping)
                return positionsToClick;

            List < List<int[]> > groupsOfPos = new List<List<int[]>>();

            groupsOfPos.Add(positionsToClick);
            while (groupsOfPos.Count > 0)
            {
                for (int i = 0; i < groupsOfPos[0].Count; i++)
                {
                    int clickX = groupsOfPos[0][i][0];
                    int clickY = groupsOfPos[0][i][1];

                    List<int[]> newPos = clickTile(clickX, clickY, true)!;
                    if (newPos.Count > 0)
                        groupsOfPos.Add(newPos);
                }
                groupsOfPos.RemoveAt(0);
            }

            return null;
        }

        public void altClick(int x, int y, bool middleClick)
        {
            if (!clickedYet)
                return;

            MinesweeperTileState stateToAdd = MinesweeperTileState.FLAGGED;
            MinesweeperTileState stateToRemove = MinesweeperTileState.QUESTIONED;

            if (middleClick)
            {
                stateToAdd = MinesweeperTileState.QUESTIONED;
                stateToRemove = MinesweeperTileState.FLAGGED;
            }

            int gridIndex = getGridIndex(x, y);
            int gridNumber = grid[gridIndex];

            if (gridHasState(gridNumber, MinesweeperTileState.CLICKED))
                return;

            // !middleclick means to flag
            // middleclick means to question mark it

            if (gridHasState(gridNumber, stateToAdd))
            {
                // if removing flag, via double right click or question mark override
                if (!middleClick || gridHasState(gridNumber, MinesweeperTileState.FLAGGED))
                {
                    if (!gridHasState(gridNumber, MinesweeperTileState.BOMB))
                        tilesToRefreshWhenDead.Remove(buttonGrid[gridIndex]);
                    bombsLeftNumberForDrawing++;
                }

                gridNumber = gridRemoveState(gridNumber, stateToRemove);
                gridNumber = gridRemoveState(gridNumber, stateToAdd);
            }
            else if (bombsLeftNumberForDrawing > 0 || middleClick)
            {
                if (middleClick && gridHasState(gridNumber, MinesweeperTileState.FLAGGED))
                {
                    if (!gridHasState(gridNumber, MinesweeperTileState.BOMB))
                        tilesToRefreshWhenDead.Remove(buttonGrid[gridIndex]);
                    bombsLeftNumberForDrawing++;
                }

                gridNumber = gridRemoveState(gridNumber, stateToRemove);
                gridNumber |= (int)stateToAdd;

                if (!middleClick)
                {
                    bombsLeftNumberForDrawing--;
                    tilesToRefreshWhenDead.Add(buttonGrid[gridIndex]);
                }
            }
            grid[gridIndex] = gridNumber;
            refreshGridTile(x, y);
            refreshMineCount();
        }

        #endregion

        #region ui stuff

        // - 1 because idk???
        const int buttonWidth = 31;
        const int buttonHeight = 31;

        public void mouseDownForm(object? sender, EventArgs e)
        {
            refreshFace(!baseFace ? null : Properties.Resources.facetile);
        }
        public void mouseUpForm(object? sender, EventArgs e)
        {
            refreshFace();
        }

        public void facePressed(object? sender, EventArgs e)
        {  
            refreshFace(Properties.Resources.facepressed);
        }

        public void faceMouseUp(object? sender, EventArgs e)
        {
            refreshFace();
            if (!clickedYet)
                return;
            init();
            timer.Stop();
            // secondPassed does ++ on this
            secondsPassed = -1;
            secondPassed(null, new EventArgs());
        }
        public void secondPassed(object? sender, EventArgs e)
        {
            if (secondsPassed < 999)
                secondsPassed++;
            int hundredthMines = (int)Math.Floor(secondsPassed / 100f);
            int tenthMines = (int)Math.Floor(secondsPassed / 10f) % 10;
            int onethMines = secondsPassed % 10;

            secondsHundreds.Image = getNumImg(hundredthMines);
            secondsTens.Image = getNumImg(tenthMines);
            secondsOnes.Image = getNumImg(onethMines);
        }
        public void refreshFace(Image? forceSet = null)
        {
            face.Image = forceSet != null ? forceSet : faceImage;
        }

        public NButton createButton(int x, int y, Image img)
        {
            NButton button = new NButton();

            // do that and this
            button.Location = new Point(x, y);
            button.Image = img;
            button.Size = new Size(buttonWidth, buttonHeight);

            Controls.Add(button);

            return button;
        }

        public void buttonClickFunc(object? s, EventArgs eventArgs)
        {
            if (s == null || gameState != MinesweeperGameEndType.NONE)
                return;
            NButton sender = (NButton)s;

            MouseEventArgs mouseEventArgs = (MouseEventArgs)eventArgs;
            int[] pos = ((TagBullshit)sender.Tag!).pos;


            switch (mouseEventArgs.Button)
            {
                // do nothing
                default: break;
                // do something
                case MouseButtons.Left:
                    clickTileCheck(pos[0], pos[1]);
                    break;
                case MouseButtons.Right:
                case MouseButtons.Middle:
                    altClick(pos[0], pos[1], mouseEventArgs.Button == MouseButtons.Middle);
                    break;
            }
        }

        public void refreshGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                    refreshGridTile(x, y);
            }
        }

        public void refreshGridTile(int x, int y)
        {
            int i = getGridIndex(x, y);
            int gridNumber = grid[i];
            NButton button = buttonGrid[i];
            bool flag = false;

            if (gridHasState(gridNumber, MinesweeperTileState.CLICKED))
                button.Image = getTileNumImg(gridNumber & 15);
            else if (gridHasState(gridNumber, MinesweeperTileState.FLAGGED))
            {
                button.Image = Properties.Resources.tileflag;
                flag = true;
            }
            else if (gridHasState(gridNumber, MinesweeperTileState.QUESTIONED))
                button.Image = Properties.Resources.tilequestioned;
            else
                button.Image = Properties.Resources.tile;

            // specific stuff for game lose
            if (gameState == MinesweeperGameEndType.GAMEOVER)
            {
                if (gridHasState(gridNumber, MinesweeperTileState.BOMB_CLICKED))
                    button.Image = Properties.Resources.tilebombclicked;
                else
                {
                    bool hasBomb = gridHasState(gridNumber, MinesweeperTileState.BOMB);

                    if (flag && !hasBomb)
                        button.Image = Properties.Resources.tilenobomb;
                    else if (!flag && hasBomb)
                        button.Image = Properties.Resources.tilemine;
                }
            }
            else if (gameState == MinesweeperGameEndType.GAMEWIN)
            {
                if (gridHasState(gridNumber, MinesweeperTileState.BOMB))
                    button.Image = Properties.Resources.tileflag;
            }
        }

        public void refreshMineCount()
        {
            int bombTemp = Math.Min(bombsLeftNumberForDrawing, 999);
        
            int hundredthMines = (int)Math.Floor(bombTemp / 100f);
            int tenthMines = (int)Math.Floor(bombTemp / 10f) % 10;
            int onethMines = bombTemp % 10;

            minesHundreds.Image = getNumImg(hundredthMines);
            minesTens.Image = getNumImg(tenthMines);
            minesOnes.Image = getNumImg(onethMines);
        }

        public void createButtonGrid()
        {
            int offsetX = 24;
            int offsetY = 108;
            buttonGrid = new NButton[gridWidth * gridHeight];
            for (int i = 0; i < grid.Length; i++)
            {
                int modX = i % gridWidth;
                int y = (int)Math.Floor((double)(i / gridWidth));
                NButton button = createButton(modX * (buttonWidth) + offsetX, y * (buttonHeight) + offsetY, Properties.Resources.tile);
                button.Tag = new TagBullshit([modX, y]);
                button.MouseDown += buttonClickFunc;

                // I am not learning allat
                // https://stackoverflow.com/questions/19654372/disabling-the-hover-behavior-on-winform-buttons
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.MouseOverBackColor = button.BackColor;

                // lucky guess
                button.FlatAppearance.BorderSize = 0;

                buttonGrid[i] = button;
            }
        }

        #endregion

        #region horrible functions for the ui


        void _utilRect(Control con, int x, int y, int w, int h, Color col)
        {
            con.Location = new Point(x, y);
            con.Size = new Size(w, h);
            con.BackColor = col;
        }
        RectForMine makeRectForMine(int x, int y, int w, int h, int thickness, Image? corner, bool solidCol = false)
        {
            Color leftLinesCol = Color.FromArgb(255 << 24 | 128 << 16 | 128 << 8 | 128);
            Color rightLinesCol = solidCol ? leftLinesCol : Color.White;

            int cornerW = corner != null ? corner.Width : 2;
            int cornerH = corner != null ? corner.Height : 2;

            // Corners will fill this empty hole
            w -= cornerW;
            h -= cornerH;

            RectForMine rect = default(RectForMine);

            // left lines
            rect.topLine = new PictureBox();
            _utilRect(rect.topLine, x, y, w, thickness, leftLinesCol);

            rect.leftLine = new PictureBox();
            _utilRect(rect.leftLine, x, y, thickness, h, leftLinesCol);


            // right lines
            rect.rightLine = new PictureBox();
            _utilRect(rect.rightLine, x + w, y + cornerH, thickness, h, rightLinesCol);

            rect.bottomLine = new PictureBox();
            _utilRect(rect.bottomLine, x + cornerW, y + h, w, thickness, rightLinesCol);

            Controls.Add(rect.topLine);
            Controls.Add(rect.leftLine);
            Controls.Add(rect.rightLine);
            Controls.Add(rect.bottomLine);

            if (corner != null)
            {
                rect.topCorner = new PictureBox();
                rect.topCorner.Image = corner;
                rect.topCorner.Location = new Point(x + w, y);

                rect.bottomCorner = new PictureBox();
                rect.bottomCorner.Image = corner;
                rect.bottomCorner.Location = new Point(x, y + h);
                rect.bottomCorner.Size = new Size(cornerW, cornerH);

                Controls.Add(rect.topCorner);
                Controls.Add(rect.bottomCorner);
            }

            if (solidCol)
            {
                rect.topLine.SendToBack();
                rect.rightLine.SendToBack();
                rect.leftLine.SendToBack();
                rect.bottomLine.SendToBack();
            }

            return rect;
        }

        Image getTileNumImg(int num)
        {
            Image returning;
            switch (num)
            {
                default: returning = Properties.Resources.tile; break;
                case 0: returning = Properties.Resources.tile0; break;
                case 1: returning = Properties.Resources.tile1; break;
                case 2: returning = Properties.Resources.tile2; break;
                case 3: returning = Properties.Resources.tile3; break;
                case 4: returning = Properties.Resources.tile4; break;
                case 5: returning = Properties.Resources.tile5; break;
                case 6: returning = Properties.Resources.tile6; break;
                case 7: returning = Properties.Resources.tile7; break;
                case 8: returning = Properties.Resources.tile8; break;
            }
            return returning;
        }

        Image getNumImg(int num)
        {
            Image returning;
            switch (num)
            {
                default: returning = Properties.Resources.numblank; break;
                case 0: returning = Properties.Resources.num0; break;
                case 1: returning = Properties.Resources.num1; break;
                case 2: returning = Properties.Resources.num2; break;
                case 3: returning = Properties.Resources.num3; break;
                case 4: returning = Properties.Resources.num4; break;
                case 5: returning = Properties.Resources.num5; break;
                case 6: returning = Properties.Resources.num6; break;
                case 7: returning = Properties.Resources.num7; break;
                case 8: returning = Properties.Resources.num8; break;
                case 9: returning = Properties.Resources.num9; break;
            }
            return returning;
        }

        #endregion
    }
}
// https://stackoverflow.com/questions/32823525/how-to-stop-pressing-button-using-keyboard-keys-like-spacebar-or-enter-c-shar
public class NButton : Button
{
    public NButton()
    {
        SetStyle(ControlStyles.Selectable, false);
    }
}

public class TagBullshit
{
    public int[] pos;
    public TagBullshit(int[] pos)
    { this.pos = pos; }
}

// so bad but whatever
public struct RectForMine
{
    public PictureBox topLine;
    public PictureBox leftLine;
    public PictureBox rightLine;
    public PictureBox bottomLine;

    public PictureBox? topCorner;
    public PictureBox? bottomCorner;
}

// Make 4 bits of space for us to include the number of bombs
// Why am i doing it like this?
// Good question
public enum MinesweeperTileState
{
    NOT_CLICKED = 1 << 4,
    FLAGGED = 2 << 4,
    QUESTIONED = 4 << 4,
    CLICKED = 8 << 4,
    BOMB = 16 << 4,
    BOMB_CLICKED = 32 << 4,
}

public enum MinesweeperGameEndType
{
    NONE = 0,
    GAMEWIN = 1,
    GAMEOVER = 2,
}
