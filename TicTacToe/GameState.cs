using System;

namespace TicTacToe
{
    public class GameState
    {
        // Свойства для представления состояния игры
        public Player[,] GameGrid { get; private set; }  // Игровое поле
        public Player CurrentPlayer { get; private set; }  // Текущий игрок
        public int TurnsPassed { get; private set; }  // Количество ходов
        public bool GameOver { get; private set; }  // Завершена ли игра

        // События, которые можно подписывать на изменения в состоянии игры
        public event Action<int, int> MoveMade;  // Событие для совершения хода
        public event Action<GameResult> GameEnded;  // Событие для окончания игры
        public event Action GameRestarted;  // Событие для перезапуска игры

        // Конструктор класса
        public GameState()
        {
            GameGrid = new Player[3, 3];
            CurrentPlayer = Player.X;  // Игра начинается с игрока X
            TurnsPassed = 0;
            GameOver = false;
        }

        // Приватные вспомогательные методы

        // Метод проверки возможности совершения хода
        private bool CanMakeMove(int r, int c)
        {
            return !GameOver && GameGrid[r, c] == Player.None;
        }

        // Метод проверки, заполнено ли игровое поле
        private bool IsGridFull()
        {
            return TurnsPassed == 9;
        }

        // Метод для смены текущего игрока
        private void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.X ? Player.O : Player.X;
        }

        // Метод для проверки, все ли клетки в указанной последовательности принадлежат игроку
        private bool AreSquaresMarked((int, int)[] squares, Player player)
        {
            foreach ((int r, int c) in squares)
            {
                if (GameGrid[r, c] != player)
                {
                    return false;
                }
            }
            return true;
        }

        // Метод для проверки, выиграл ли игрок после совершения хода
        private bool DidMoveWin(int r, int c, out WinInfo winInfo)
        {
            // Определение последовательностей, которые могут привести к победе
            (int, int)[] row = new[] { (r, 0), (r, 1), (r, 2) };
            (int, int)[] col = new[] { (0, c), (1, c), (2, c) };
            (int, int)[] mainDiag = new[] { (0, 0), (1, 1), (2, 2) };
            (int, int)[] antiDiag = new[] { (0, 2), (1, 1), (2, 0) };

            // Проверка, выиграна ли игра в одной из последовательностей
            if (AreSquaresMarked(row, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Row, Number = r };
                return true;
            }

            if (AreSquaresMarked(col, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Column, Number = c };
                return true;
            }

            if (AreSquaresMarked(mainDiag, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.MainDiagonal };
                return true;
            }

            if (AreSquaresMarked(antiDiag, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.AntiDiagonal };
                return true;
            }

            winInfo = null;
            return false;
        }

        // Метод для проверки завершения игры после совершения хода
        private bool DidMoveEndGame(int r, int c, out GameResult gameResult)
        {
            if (DidMoveWin(r, c, out WinInfo winInfo))
            {
                gameResult = new GameResult { Winner = CurrentPlayer, WinInfo = winInfo };
                return true;
            }

            if (IsGridFull())
            {
                gameResult = new GameResult { Winner = Player.None };
                return true;
            }

            gameResult = null;
            return false;
        }

        // Метод для совершения хода
        public void MakeMove(int r, int c)
        {
            if (!CanMakeMove(r, c))
            {
                return;  // Нельзя сделать ход, если игра завершена или клетка занята
            }

            GameGrid[r, c] = CurrentPlayer;  // Заполнение клетки текущим игроком
            TurnsPassed++;  // Увеличение счетчика ходов

            // Проверка, завершена ли игра после хода
            if (DidMoveEndGame(r, c, out GameResult gameResult))
            {
                GameOver = true;
                MoveMade?.Invoke(r, c);
                GameEnded?.Invoke(gameResult);
            }
            else
            {
                SwitchPlayer();  // Смена текущего игрока
                MoveMade?.Invoke(r, c);
            }
        }

        // Метод для сброса состояния игры
        public void Reset()
        {
            GameGrid = new Player[3, 3];  // Очистка игрового поля
            CurrentPlayer = Player.X;  // Начало с игрока X
            TurnsPassed = 0;  // Сброс счетчика ходов
            GameOver = false;  // Сброс флага завершения игры
            GameRestarted?.Invoke();  // Вызов события перезапуска игры
        }
    }
}