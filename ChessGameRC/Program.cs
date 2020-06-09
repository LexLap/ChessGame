using System;

namespace ChessGames
{
    class ChessGame : Geometrics
    {
        static void Main(string[] args)
        {

            char playerColorTurn;
            Pieces[,] board;
            string[,] currentBoardSnapshot;
            board = initBoard();

            bool gameOver = false;
            while (!gameOver)
            {
                currentBoardSnapshot = printBoard(board); //both print and create a snapshot of the board

                if (moveCounter % 2 == 0)
                    playerColorTurn = 'W';
                else playerColorTurn = 'B';

                if (isCheck(playerColorTurn, board))
                    Console.WriteLine((playerColorTurn == 'W' ? "White" : "Black") + " player in Check!");
                if ((!checkIfAnyPieceMovable(playerColorTurn, board)) && isCheck(playerColorTurn, board))
                {
                    Console.WriteLine("Check and Mate! " + (playerColorTurn == 'B' ? "White" : "Black") + " player has won the game!");
                    break;
                }
                if (!checkIfAnyPieceMovable(playerColorTurn, board))
                {
                    Console.WriteLine("Stalemate! The game ended by a draw.");
                    break;
                }
                if (insufficientMaterial(board))
                {
                    Console.WriteLine("'Insufficient Material' situation! The game ended by a draw.");
                    break;
                }
                if (checkIfThreeFoldRepitition(currentBoardSnapshot))
                {
                    Console.WriteLine("The game was ended in a draw by the 'Threefold Repitition' rule");
                    break;
                }
                if (checkIfFiftyMove())
                {
                    Console.WriteLine("The game was ended in a draw by the 'Fifty Move' rule");
                    break;
                }
                if (!executePlayerMove(playerColorTurn, board)) //returns false and ends the game by player decision 
                    gameOver = true;
            }
            Console.ReadKey();//prevent closing the console

        }

        static string[][,] boardSnapshots = new string[9][,];
        static int fiftyMoveCounter = 0;
        public static int moveCounter = 0;

        public static bool insufficientMaterial(Pieces[,] board)
        {
            int blackKnights = 0;
            int blackOnBlackBishops = 0;
            int blackOnWhiteBishops = 0;
            int whiteKnights = 0;
            int whiteOnBlackBishops = 0;
            int whiteOnWhiteBishops = 0;
            for (int y = 1; y < 9; y++)
                for (int x = 1; x < 9; x++)
                {
                    if ((board[x, y] is Queen) || (board[x, y] is Pawn) || (board[x, y] is Rook))
                        return false;
                    if (board[x, y] is Knight)
                        if (board[x, y].getColor() == 'B')
                            blackKnights++;
                        else whiteKnights++;
                    if (board[x, y] is Bishop)
                    {
                        if (board[x, y].getColor() == 'B')
                        {
                            if ((x + y) % 2 == 0)
                                blackOnWhiteBishops++;
                            else blackOnBlackBishops++;
                        }
                        else
                        {
                            if ((x + y) % 2 == 0)
                                whiteOnWhiteBishops++;
                            else whiteOnBlackBishops++;
                        }
                    }
                }

            if (blackKnights + whiteKnights > 1)
                return false;
            if (blackKnights + blackOnBlackBishops + blackOnWhiteBishops + whiteKnights + whiteOnBlackBishops + whiteOnWhiteBishops == 0)
                return true;
            if (blackKnights + whiteKnights == 0)
            {
                if (blackOnBlackBishops + blackOnWhiteBishops + whiteOnBlackBishops + whiteOnWhiteBishops == 1)
                    return true;
                if (blackOnBlackBishops == 1 && blackOnWhiteBishops == 0 && whiteOnBlackBishops == 1 && whiteOnWhiteBishops == 0)
                    return true;
                if (blackOnBlackBishops == 0 && blackOnWhiteBishops == 1 && whiteOnBlackBishops == 0 && whiteOnWhiteBishops == 1)
                    return true;
            }
            if (blackKnights + whiteKnights == 1)
                if (blackOnBlackBishops + blackOnWhiteBishops + whiteOnBlackBishops + whiteOnWhiteBishops == 0)
                    return true;
            return false;
        }
        static bool emulateNextMove(Pieces[,] board, Pieces piece, int newIndexX, int newIndexY)
        {
            int oldIndexX = piece.getCoordX();
            int oldIndexY = piece.getCoordY();
            Pieces[,] virtualBoard;
            virtualBoard = (Pieces[,])board.Clone();
            virtualBoard[newIndexX, newIndexY] = piece;
            virtualBoard[oldIndexX, oldIndexY] = null;
            if (isCheck(piece.getColor(), virtualBoard))
                return false;
            return true;
        }
        static bool isCheck(char color, Pieces[,] board)
        {
            int kingCoordX = 0;
            int kingCoordY = 0;
            for (int y = 1; y < 9; y++)
                for (int x = 1; x < 9; x++)
                    if (board[x, y] is King)
                        if (board[x, y].getColor() == color)
                        {
                            kingCoordX = x;
                            kingCoordY = y;
                        }
            for (int y = 1; y < 9; y++)
                for (int x = 1; x < 9; x++)
                    if (!(board[x, y] == null))
                        if (isValidMove(board, board[x, y], kingCoordX, kingCoordY))
                            return true;
            return false;
        }
        static bool executePlayerMove(char color, Pieces[,] board)
        {

            Pieces piece = getPiece(color, board);
            if (piece is null)
                return false;
            movePiece(piece, board);
            return true;

        }
        static bool checkIfAnyPieceMovable(char color, Pieces[,] board)
        {
            for (int a = 1; a < 9; a++)
                for (int b = 1; b < 9; b++)
                    for (int y = 1; y < 9; y++)
                        for (int x = 1; x < 9; x++)
                            if (!(board[x, y] is null))
                                if (board[x, y].getColor() == color)
                                    if (isValidMove(board, board[x, y], a, b))
                                        if (emulateNextMove(board, board[x, y], a, b))
                                            return true;
            return false;
        }
        static bool checkIfPieceMovable(Pieces[,] board, Pieces piece)
        {
            bool isMovable = false;
            for (int y = 1; y < 9; y++)
                for (int x = 1; x < 9; x++)
                {
                    if (isValidMove(board, piece, x, y))
                    {
                        if (emulateNextMove(board, piece, x, y))
                        {
                            Console.WriteLine("Hint: possible move to " + x + returnCharValue(y));
                            isMovable = true;
                        }
                    }
                }
            return isMovable;
        }
        static bool movePiece(Pieces piece, Pieces[,] board)//only movable piece will achieve this step
        {
            int oldXCoord = piece.getCoordX();
            int oldYCoord = piece.getCoordY();
            string oldCharY = returnCharValue(oldYCoord) + "";
            oldCharY = oldCharY.ToLower();
            Console.WriteLine("Please enter the coordinates where you wish to move your piece: ");
            bool succeedMove = false;
            while (!succeedMove) //will keep asking for new coordinates untill valid one received
            {
                string coord = getUserInput();
                int newIndexY = returnIntValue(coord[0]);
                int newIndexX = int.Parse(coord[1] + "");
                if (isValidMove(board, piece, newIndexX, newIndexY) && emulateNextMove(board, piece, newIndexX, newIndexY)) //checking if move is possible and wont cause check
                {
                    if ((piece is Pawn) && (!piece.hasMoved))//checking for En Passant
                        checkIfEnPassant(piece, board, newIndexX, newIndexY);

                    if (piece is Pawn) //in En Passant situation the passer is being removed
                        if ((moveCounter - 1) == piece.getMoveId())
                            if (newIndexY == piece.getPasserIndexY())
                                board[piece.getPasserIndexX(), piece.getPasserIndexY()] = null;

                    if (piece is King)//executing castling
                    {
                        if (newIndexY - oldYCoord == 2)
                            executeLongCastling(board, newIndexX);
                        if (newIndexY - oldYCoord == (-2))
                            executeShortCastling(board, newIndexX);
                    }

                    if (!(piece is Pawn) && (board[newIndexX, newIndexY] is null))
                        fiftyMoveCounter++;
                    else fiftyMoveCounter = 0;

                    board[newIndexX, newIndexY] = piece;
                    board[newIndexX, newIndexY].setCoordX(newIndexX);
                    board[newIndexX, newIndexY].setCoordY(newIndexY);
                    board[newIndexX, newIndexY].setMoved();
                    board[oldXCoord, oldYCoord] = null;

                    moveCounter++;
                    Console.WriteLine("Successfully moved {0} from {1} to {2}", piece, oldCharY + oldXCoord, coord);
                    if ((piece is Pawn) && ((newIndexX == 8) || (newIndexX == 1)))
                        evaluatePawn(board, piece);
                    return true;
                }
                else Console.WriteLine("Cannot place the piece here! Please enter a new coordinates: ");
            }
            return false;
        }
        static bool isValidMove(Pieces[,] board, Pieces piece, int newIndexX, int newIndexY)
        {
            if (isSelfPiece(piece, board, newIndexX, newIndexY))
                return false;
            if (piece.isValidMove(board, newIndexX, newIndexY))
                return true;
            return false;
        }
        static bool checkIfThreeFoldRepitition(string[,] currentBoardSnapshot)
        {
            for (int i = 0; i < 8; i++)
                boardSnapshots[i] = boardSnapshots[i + 1];
            boardSnapshots[8] = currentBoardSnapshot;

            if (boardSnapshots[0] != null)
            {
                for (int row = 1; row < 9; row++)
                    for (int column = 1; column < 9; column++)
                        if ((!(boardSnapshots[0][row, column] == boardSnapshots[4][row, column] && boardSnapshots[4][row, column] == boardSnapshots[8][row, column]))
                            || (!(boardSnapshots[1][row, column] == boardSnapshots[5][row, column]))
                            || (!(boardSnapshots[2][row, column] == boardSnapshots[6][row, column]))
                            || (!(boardSnapshots[3][row, column] == boardSnapshots[7][row, column]))
                            )
                            return false;
                return true;
            }
            return false;
        }
        static bool checkIfFiftyMove()
        {
            if (fiftyMoveCounter >= 100)
                return true;
            return false;
        }
        static bool initiateDraw(char color)
        {

            if (moveCounter >= 3)
            {
                Console.WriteLine((color == 'W' ? "White" : "Black") + " player claims for a draw. ");
                Console.WriteLine((color == 'W' ? "Black" : "White") + " player, please enter 'y' to agree or any other button to refuse: ");
                string answer = Console.ReadLine();
                if (answer == "y")
                {
                    Console.WriteLine("The game was ended by a draw agreement!");
                    return true;
                }
            }
            else Console.WriteLine("Draw requirements have not met, minimum move count is 3.");
            return false;
        }
        static bool initiateResign(char color)
        {
            Console.WriteLine((color == 'W' ? "White" : "Black") + " player, please enter 'y' to confirm the resign or any other button to cancel: ");

            string answer = Console.ReadLine();
            if (answer == "y")
            {
                Console.WriteLine((color == 'W' ? "White" : "Black") + " player has resigned, " + (color == 'W' ? "Black" : "White") + " player is victorious!");
                return true;
            }
            return false;
        }
        static Pieces getPiece(char color, Pieces[,] board)
        {
            Pieces piece = new Pieces();
            Console.WriteLine((color == 'W' ? "White" : "Black") + " player, this is your move!");
            bool validPiece = false;
            while (!validPiece)
            {
                Console.WriteLine("Please enter the coordinates for the piece you wish to move or 'draw' / 'resign' : ");
                string coord = getUserInput();
                if (coord == "draw")
                    if (initiateDraw(color))
                        return null;
                    else continue;
                if (coord == "resign")
                    if (initiateResign(color))
                        return null;
                    else continue;
                int indexY = returnIntValue(coord[0]);
                int indexX = int.Parse(coord[1] + "");

                if (board[indexX, indexY] != null)
                {
                    piece = board[indexX, indexY];
                    if (piece.getColor() == color)
                    {
                        if (checkIfPieceMovable(board, piece))
                            validPiece = true;
                        else Console.WriteLine("This piece cannot move, please choose another piece!");
                    }
                    else Console.WriteLine("You cannot move opposite's pieces, please choose another piece!");
                }
                else Console.WriteLine("You have no piece in that cell!");
            }
            return piece;
        }
        static void evaluatePawn(Pieces[,] board, Pieces piece)
        {
            Console.WriteLine("Congratulations! Your Pawn has made it! Please choose an evaluation option, for example ('Q'/'R'/'B'/'N'): ");
            string choises = "QRBNqrbn";
            char newType = ' ';
            bool inputReceived = false;
            while (!inputReceived)
            {
                string input = Console.ReadLine();
                if (input.Length == 1)
                    for (int i = 0; i < choises.Length; i++)
                        if (choises[i] == input[0])
                        {
                            input = input.ToUpper();
                            newType = input[0];
                            inputReceived = true;
                        }
                if (!inputReceived)
                    Console.WriteLine("You have entered a wrong letter, please try again!");
            }

            char color = piece.getColor();
            int coordX = piece.getCoordX();
            int coordY = piece.getCoordY();

            switch (newType)
            {
                case 'Q': board[coordX, coordY] = new Queen(color, newType); break;
                case 'R': board[coordX, coordY] = new Rook(color, newType); break;
                case 'B': board[coordX, coordY] = new Bishop(color, newType); break;
                case 'N': board[coordX, coordY] = new Knight(color, newType); break;
            }
            board[coordX, coordY].setCoordX(coordX);
            board[coordX, coordY].setCoordY(coordY);
        }
        static void executeShortCastling(Pieces[,] board, int newIndexX)
        {
            board[newIndexX, 3] = board[newIndexX, 1];
            board[newIndexX, 1] = null;
        }
        static void executeLongCastling(Pieces[,] board, int newIndexX)
        {
            board[newIndexX, 5] = board[newIndexX, 8];
            board[newIndexX, 8] = null;
        }
        static void checkIfEnPassant(Pieces piece, Pieces[,] board, int newIndexX, int newIndexY)
        {
            int oldIndexX = piece.getCoordX();
            int oldIndexY = piece.getCoordY();
            char color = piece.getColor();
            int sideMod;
            sideMod = oldIndexX < newIndexX ? 2 : (-2);
            if (oldIndexX + sideMod == newIndexX)
            {
                // If enemy's Pawn present, assign it data regarding the passer, move id and locations

                if (board[oldIndexX + sideMod, oldIndexY + 1] is Pawn)
                    if (!(color == board[oldIndexX + sideMod, oldIndexY + 1].getColor()))
                        ((Pawn)board[oldIndexX + sideMod, oldIndexY + 1]).setPasser(moveCounter, newIndexX, newIndexY, oldIndexX + (sideMod / 2));
                if (board[oldIndexX + sideMod, oldIndexY - 1] is Pawn)
                    if (!(color == board[oldIndexX + sideMod, oldIndexY - 1].getColor()))
                        ((Pawn)board[oldIndexX + sideMod, oldIndexY - 1]).setPasser(moveCounter, newIndexX, newIndexY, oldIndexX + (sideMod / 2));
            }
        }
        public static bool isSelfPiece(Pieces piece, Pieces[,] board, int newIndexX, int newIndexY)
        {
            if (!(board[newIndexX, newIndexY] is null))
            {
                if (board[newIndexX, newIndexY].getColor() != piece.getColor())
                    return false;
            }
            else if (board[newIndexX, newIndexY] is null)
                return false;
            return true;
        }
        static int returnIntValue(char c)
        {
            int cValue = 0;
            switch (c)
            {
                case 'h': cValue = 1; break;
                case 'g': cValue = 2; break;
                case 'f': cValue = 3; break;
                case 'e': cValue = 4; break;
                case 'd': cValue = 5; break;
                case 'c': cValue = 6; break;
                case 'b': cValue = 7; break;
                case 'a': cValue = 8; break;
            }
            return cValue;
        }
        static char returnCharValue(int c)
        {
            char cValue = ' ';
            switch (c)
            {
                case 1: cValue = 'h'; break;
                case 2: cValue = 'g'; break;
                case 3: cValue = 'f'; break;
                case 4: cValue = 'e'; break;
                case 5: cValue = 'd'; break;
                case 6: cValue = 'c'; break;
                case 7: cValue = 'b'; break;
                case 8: cValue = 'a'; break;
            }
            return cValue;
        }
        static string getUserInput()
        {
            string coord = "";
            bool gotCoord = false;
            while (!gotCoord)
            {
                coord = Console.ReadLine();
                if ((coord == "draw") || (coord == "resign"))
                    return coord;
                if (coord.Length == 2)
                {
                    string s1 = "abcdefgh";
                    string s2 = "12345678";
                    coord = coord.ToLower();
                    for (int i = 0; i < s1.Length; i++)
                        if (coord[1] == s1[i])
                            for (int y = 0; y < s2.Length; y++)
                                if (coord[0] == s2[y])
                                {
                                    coord = coord[1] + (coord[0] + "");
                                    gotCoord = true;
                                }
                    for (int i = 0; i < s1.Length; i++)
                        if (coord[0] == s1[i])
                            for (int y = 0; y < s2.Length; y++)
                                if (coord[1] == s2[y])
                                {
                                    coord = coord[0] + (coord[1] + "");
                                    gotCoord = true;
                                }
                }
                if (!gotCoord)
                    Console.WriteLine("You have entered a wrong coordinates! Please try again: ");
            }
            return coord;
        }
        static string[,] printBoard(Pieces[,] board)
        {
            string[,] currentBoardSnapshot = new string[10, 10];
            Console.WriteLine("\n\n");
            for (int row = 0; row < 10; row++)
            {
                for (int column = 0; column < 10; column++)
                {
                    if (board[row, column] == null)
                        Console.Write("    ");
                    else
                    {
                        Console.Write("  " + board[row, column]);
                        currentBoardSnapshot[row, column] = board[row, column].ToString();
                    }
                }
                Console.WriteLine("\n");
            }
            Console.WriteLine("\n");
            return currentBoardSnapshot;
        }
        static Pieces[,] initBoard()
        {
            Pieces[,] board = new Pieces[10, 10] {
                {new Border(' '), new Border('h'),new Border('g'),new Border('f'),new Border('e'),new Border('d'),new Border('c'),new Border('b'),new Border('a'),new Border(' ')},
                {new Border('1'),new Rook('W', 'R'),new Knight('W', 'N'),new Bishop('W', 'B'),new King('W', 'K'),new Queen('W', 'Q'),new Bishop('W', 'B'),new Knight('W', 'N'),new Rook('W', 'R'),new Border('1')},
                {new Border('2'),new Pawn('W', 'P'),new Pawn('W', 'P'),new Pawn('W', 'P'),new Pawn('W', 'P'),new Pawn('W', 'P'),new Pawn('W', 'P'),new Pawn('W', 'P'),new Pawn('W', 'P'),new Border('2')},
                {new Border('3'), null,null,null,null,null,null,null,null,new Border('3')},
                {new Border('4'), null,null,null,null,null,null,null,null,new Border('4')},
                {new Border('5'), null,null,null,null,null,null,null,null,new Border('5')},
                {new Border('6'), null,null,null,null,null,null,null,null,new Border('6')},
                {new Border('7'),new Pawn('B', 'P'),new Pawn('B', 'P'),new Pawn('B', 'P'),new Pawn('B', 'P'),new Pawn('B', 'P'),new Pawn('B', 'P'),new Pawn('B', 'P'),new Pawn('B', 'P'),new Border('7')},
                {new Border('8'), new Rook('B', 'R'),new Knight('B', 'N'),new Bishop('B', 'B'),new King('B', 'K'),new Queen('B', 'Q'),new Bishop('B', 'B'),new Knight('B', 'N'),new Rook('B', 'R'),new Border('8')},
                {new Border(' '), new Border('h'),new Border('g'),new Border('f'),new Border('e'),new Border('d'),new Border('c'),new Border('b'),new Border('a'),new Border(' ')}
            };
            for (int y = 1; y < 9; y++)
                for (int x = 1; x < 9; x++)
                    if (!(board[x, y] is null))
                    {
                        board[x, y].setCoordX(x);
                        board[x, y].setCoordY(y);
                    }
            return board;
        }
    }
    class Pieces : ChessGame
    {
        public bool hasMoved;
        int coordX;
        int coordY;
        char color;
        protected char name;

        public Pieces(char color, char name)
        {
            this.color = color;
            this.name = name;
        }
        public Pieces(char name)
        {
            this.name = name;
        }
        public Pieces() { }
        public override string ToString()
        {
            string result = "" + color + name;
            return result;
        }

        virtual public bool isValidMove(Pieces[,] board, int newIndexX, int newIndexY) { return false; }
        virtual public int getMoveId() { return 0; }
        virtual public int getPasserIndexY() { return 0; }
        virtual public int getPasserIndexX() { return 0; }

        public char getColor()
        {
            return color;
        }
        public int getCoordX()
        {
            return coordX;
        }
        public int getCoordY()
        {
            return coordY;
        }
        public void setCoordX(int x) { coordX = x; }
        public void setCoordY(int y) { coordY = y; }
        public void setMoved() { hasMoved = true; }

    }
    class Pawn : Pieces
    {
        int moveId;
        public int passerIndexX;
        public int passerIndexY;
        int jumpToX;
        public void setPasser(int moveId, int passerIndexX, int passerIndexY, int jumpToX)
        {
            this.moveId = moveId;
            this.passerIndexX = passerIndexX;
            this.passerIndexY = passerIndexY;
            this.jumpToX = jumpToX;
        }
        public override int getMoveId()
        {
            return moveId;
        }
        public override int getPasserIndexY()
        {
            return passerIndexY;
        }
        public override int getPasserIndexX()
        {
            return passerIndexX;
        }
        public Pawn(char color, char name) : base(color, name) { }
        public override bool isValidMove(Pieces[,] board, int newIndexX, int newIndexY)
        {
            if (moveCounter == (moveId + 1))
                if ((newIndexX == jumpToX) && (newIndexY == passerIndexY))
                    return true;

            if ((getCoordY() == newIndexY) && (board[newIndexX, newIndexY] is null))
                if (checkVerticalPath(board, this, newIndexX, newIndexY))
                {
                    if (getColor() == 'B')
                        if (hasMoved ? (getCoordX() - newIndexX == 1) : (getCoordX() - newIndexX == 1) || (getCoordX() - newIndexX == 2))
                            return true;
                    if (getColor() == 'W')
                        if (hasMoved ? (newIndexX - getCoordX() == 1) : (newIndexX - getCoordX() == 1) || (newIndexX - getCoordX() == 2))
                            return true;
                }
            if (getCoordY() != newIndexY)
            {
                if (getColor() == 'B')
                    if ((Math.Abs(getCoordY() - newIndexY) == 1) && (getCoordX() - newIndexX == 1))
                        if (!(board[newIndexX, newIndexY] is null))
                            if (board[newIndexX, newIndexY].getColor() != getColor())
                                return true;
                if (getColor() == 'W')
                    if ((Math.Abs(getCoordY() - newIndexY) == 1) && (newIndexX - getCoordX() == 1))
                        if (!(board[newIndexX, newIndexY] is null))
                            if (board[newIndexX, newIndexY].getColor() != getColor())
                                return true;
            }
            return false;
        }
    }
    class Bishop : Pieces
    {
        public Bishop(char color, char name) : base(color, name) { }
        public override bool isValidMove(Pieces[,] board, int newIndexX, int newIndexY)
        {
            if (checkDiagonalPath(board, this, newIndexX, newIndexY))
                return true;
            return false;
        }
    }
    class Rook : Pieces
    {
        public Rook(char color, char name) : base(color, name) { }
        public override bool isValidMove(Pieces[,] board, int newIndexX, int newIndexY)
        {
            if (checkVerticalPath(board, this, newIndexX, newIndexY) || checkHorizontalPath(board, this, newIndexX, newIndexY))
                return true;
            return false;
        }
    }
    class Queen : Pieces
    {
        public Queen(char color, char name) : base(color, name) { }
        public override bool isValidMove(Pieces[,] board, int newIndexX, int newIndexY)
        {
            if (checkVerticalPath(board, this, newIndexX, newIndexY) || checkHorizontalPath(board, this, newIndexX, newIndexY) || checkDiagonalPath(board, this, newIndexX, newIndexY))
                return true;
            return false;
        }
    }
    class Knight : Pieces
    {
        public Knight(char color, char name) : base(color, name) { }
        public override bool isValidMove(Pieces[,] board, int newIndexX, int newIndexY)
        {
            int oldIndexX = getCoordX();
            int oldIndexY = getCoordY();
            if ((((oldIndexX + 1) == newIndexX) || ((oldIndexX - 1) == newIndexX)) && ((oldIndexY + 2) == newIndexY))
                return true;
            if (((oldIndexX + 2) == newIndexX) && (((oldIndexY - 1) == newIndexY) || ((oldIndexY + 1) == newIndexY)))
                return true;
            if ((((oldIndexX - 1) == newIndexX) || ((oldIndexX + 1) == newIndexX)) && ((oldIndexY - 2) == newIndexY))
                return true;
            if (((oldIndexX - 2) == newIndexX) && (((oldIndexY + 1) == newIndexY) || ((oldIndexY - 1) == newIndexY)))
                return true;
            return false;
        }
    }
    class King : Pieces
    {
        public King(char color, char name) : base(color, name) { }
        public override bool isValidMove(Pieces[,] board, int newIndexX, int newIndexY)
        {
            if ((!hasMoved) && (newIndexX == getCoordX()))
            {
                if (!(board[newIndexX, 8] is null))
                    if ((!board[newIndexX, 8].hasMoved) && checkHorizontalPath(board, this, newIndexX, 8) && (newIndexY == 6))
                        return true;
                if (!(board[newIndexX, 1] is null))
                    if ((!board[newIndexX, 1].hasMoved) && checkHorizontalPath(board, this, newIndexX, 1) && (newIndexY == 2))
                        return true;
            }
            if ((Math.Abs(newIndexX - getCoordX()) < 2) && (Math.Abs(newIndexY - getCoordY()) < 2))
                if (checkVerticalPath(board, this, newIndexX, newIndexY) || checkHorizontalPath(board, this, newIndexX, newIndexY) || checkDiagonalPath(board, this, newIndexX, newIndexY))
                    return true;
            return false;
        }
    }
    class Border : Pieces
    {
        public Border(char name) : base(name) { }
        public override string ToString()
        {
            string result = " " + name;
            return result;
        }
    }
    class Geometrics
    {
        public static bool checkVerticalPath(Pieces[,] board, Pieces piece, int newIndexX, int newIndexY)
        {
            int oldIndexX = piece.getCoordX();
            int oldIndexY = piece.getCoordY();

            while (oldIndexX < newIndexX)
            {
                oldIndexX++;
                if ((oldIndexX == newIndexX) && (oldIndexY == newIndexY))
                    return true;
                if (!(board[oldIndexX, oldIndexY] is null))
                    return false;
            }
            while (oldIndexX > newIndexX)
            {
                oldIndexX--;
                if ((oldIndexX == newIndexX) && (oldIndexY == newIndexY))
                    return true;
                if (!(board[oldIndexX, oldIndexY] is null))
                    return false;
            }
            return false;
        }
        public static bool checkHorizontalPath(Pieces[,] board, Pieces piece, int newIndexX, int newIndexY)
        {
            int oldIndexX = piece.getCoordX();
            int oldIndexY = piece.getCoordY();
            while (oldIndexY < newIndexY)
            {
                oldIndexY++;
                if ((oldIndexX == newIndexX) && (oldIndexY == newIndexY))
                    return true;
                if (!(board[oldIndexX, oldIndexY] is null))
                    return false;
            }
            while (oldIndexY > newIndexY)
            {
                oldIndexY--;
                if ((oldIndexX == newIndexX) && (oldIndexY == newIndexY))
                    return true;
                if (!(board[oldIndexX, oldIndexY] is null))
                    return false;
            }
            return false;
        }
        public static bool checkDiagonalPath(Pieces[,] board, Pieces piece, int newIndexX, int newIndexY)
        {
            int oldIndexX = piece.getCoordX();
            int oldIndexY = piece.getCoordY();
            if ((oldIndexX > newIndexX) && (oldIndexY > newIndexY)) // x-- y--
                while ((oldIndexX > newIndexX) && (oldIndexY > newIndexY))
                {
                    oldIndexX--;
                    oldIndexY--;
                    if ((oldIndexX == newIndexX) && (oldIndexY == newIndexY))
                        return true;
                    if (!(board[oldIndexX, oldIndexY] is null))
                        return false;

                }
            if ((oldIndexX < newIndexX) && (oldIndexY < newIndexY)) // x++ y++
                while ((oldIndexX < newIndexX) && (oldIndexY < newIndexY))
                {
                    oldIndexX++;
                    oldIndexY++;
                    if ((oldIndexX == newIndexX) && (oldIndexY == newIndexY))
                        return true;
                    if (!(board[oldIndexX, oldIndexY] is null))
                        return false; ;
                }
            if ((oldIndexX > newIndexX) && (oldIndexY < newIndexY)) // x-- y++
                while ((oldIndexX > newIndexX) && (oldIndexY < newIndexY))
                {
                    oldIndexX--;
                    oldIndexY++;
                    if ((oldIndexX == newIndexX) && (oldIndexY == newIndexY))
                        return true;
                    if (!(board[oldIndexX, oldIndexY] is null))
                        return false;
                }

            if ((oldIndexX < newIndexX) && (oldIndexY > newIndexY)) // x++ y--
                while ((oldIndexX < newIndexX) && (oldIndexY > newIndexY))
                {
                    oldIndexX++;
                    oldIndexY--;
                    if ((oldIndexX == newIndexX) && (oldIndexY == newIndexY))
                        return true;
                    if (!(board[oldIndexX, oldIndexY] is null))
                        return false;
                }
            return false;
        }
    }
}
