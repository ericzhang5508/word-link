/**
 * Word-Link Crossword — Client-Side Game Logic
 * Handles grid rendering, cell interaction, keyboard navigation,
 * clue synchronization, timer, completion tracking, and SignalR.
 */
(function () {
    'use strict';

    // ─── State ───
    const gridEl = document.getElementById('crossword-grid');
    if (!gridEl) return;

    const puzzleId = parseInt(gridEl.dataset.puzzleId);
    const gridSize = parseInt(gridEl.dataset.size);
    const solutionGrid = JSON.parse(gridEl.dataset.grid);

    let userGrid = [];         // 2D array of user-entered letters
    let cellElements = [];     // 2D array of cell DOM elements
    let activeRow = -1;
    let activeCol = -1;
    let direction = 'Across';  // 'Across' or 'Down'
    let timerSeconds = 0;
    let timerInterval = null;
    let isSolved = false;

    // All clue items from DOM
    const clueItems = document.querySelectorAll('.clue-item');

    // Clue numbering map: cellNumberMap[row][col] = number (if cell has a number)
    let cellNumberMap = {};

    // ─── Initialize ───
    function init() {
        buildGrid();
        buildCellNumberMap();
        startTimer();
        bindEvents();
        initSignalR();
    }

    function buildGrid() {
        gridEl.innerHTML = '';
        userGrid = [];
        cellElements = [];

        // Determine which cells get numbers
        let clueNumber = 1;

        for (let r = 0; r < gridSize; r++) {
            userGrid[r] = [];
            cellElements[r] = [];

            for (let c = 0; c < gridSize; c++) {
                const cell = document.createElement('div');
                cell.className = 'grid-cell';
                cell.dataset.row = r;
                cell.dataset.col = c;

                if (solutionGrid[r][c] === '#') {
                    cell.classList.add('black');
                    userGrid[r][c] = '#';
                    cellElements[r][c] = cell;
                    gridEl.appendChild(cell);
                    continue;
                }

                userGrid[r][c] = '';

                // Check if this cell starts a word (Across or Down)
                const startsAcross = (c === 0 || solutionGrid[r][c - 1] === '#') &&
                                     (c + 1 < gridSize && solutionGrid[r][c + 1] !== '#');
                const startsDown = (r === 0 || solutionGrid[r - 1][c] === '#') &&
                                   (r + 1 < gridSize && solutionGrid[r + 1][c] !== '#');

                if (startsAcross || startsDown) {
                    const numEl = document.createElement('span');
                    numEl.className = 'cell-number';
                    numEl.textContent = clueNumber;
                    cell.appendChild(numEl);
                    cell.dataset.cellNumber = clueNumber;
                    clueNumber++;
                }

                const textEl = document.createElement('span');
                textEl.className = 'cell-text';
                cell.appendChild(textEl);

                cell.addEventListener('click', () => onCellClick(r, c));

                cellElements[r][c] = cell;
                gridEl.appendChild(cell);
            }
        }
    }

    function buildCellNumberMap() {
        for (let r = 0; r < gridSize; r++) {
            for (let c = 0; c < gridSize; c++) {
                const cell = cellElements[r][c];
                if (cell && cell.dataset.cellNumber) {
                    if (!cellNumberMap[r]) cellNumberMap[r] = {};
                    cellNumberMap[r][c] = parseInt(cell.dataset.cellNumber);
                }
            }
        }
    }

    // ─── Cell Interaction ───
    function onCellClick(row, col) {
        if (isSolved) return;
        if (solutionGrid[row][col] === '#') return;

        // Toggle direction if clicking same cell
        if (row === activeRow && col === activeCol) {
            direction = direction === 'Across' ? 'Down' : 'Across';
        }

        activeRow = row;
        activeCol = col;
        highlightWord();
        updateActiveClue();
    }

    function highlightWord() {
        // Clear all highlights
        for (let r = 0; r < gridSize; r++) {
            for (let c = 0; c < gridSize; c++) {
                cellElements[r][c].classList.remove('active', 'highlighted');
            }
        }

        // Clear clue list highlights
        clueItems.forEach(item => item.classList.remove('active-clue'));

        if (activeRow < 0 || activeCol < 0) return;

        // Highlight active cell
        cellElements[activeRow][activeCol].classList.add('active');

        // Highlight the word based on direction
        if (direction === 'Across') {
            // Find start of word
            let startCol = activeCol;
            while (startCol > 0 && solutionGrid[activeRow][startCol - 1] !== '#') startCol--;
            // Find end of word
            let endCol = activeCol;
            while (endCol < gridSize - 1 && solutionGrid[activeRow][endCol + 1] !== '#') endCol++;

            for (let c = startCol; c <= endCol; c++) {
                if (c !== activeCol) {
                    cellElements[activeRow][c].classList.add('highlighted');
                }
            }
        } else {
            let startRow = activeRow;
            while (startRow > 0 && solutionGrid[startRow - 1][activeCol] !== '#') startRow--;
            let endRow = activeRow;
            while (endRow < gridSize - 1 && solutionGrid[endRow + 1][activeCol] !== '#') endRow++;

            for (let r = startRow; r <= endRow; r++) {
                if (r !== activeRow) {
                    cellElements[r][activeCol].classList.add('highlighted');
                }
            }
        }

        // Highlight the corresponding clue in the list
        highlightClueItem();
    }

    function highlightClueItem() {
        // Find the clue number for the start of the current word
        let startRow = activeRow, startCol = activeCol;

        if (direction === 'Across') {
            while (startCol > 0 && solutionGrid[startRow][startCol - 1] !== '#') startCol--;
        } else {
            while (startRow > 0 && solutionGrid[startRow - 1][startCol] !== '#') startRow--;
        }

        const cellNum = cellNumberMap[startRow] && cellNumberMap[startRow][startCol];
        if (!cellNum) return;

        clueItems.forEach(item => {
            if (parseInt(item.dataset.clueNumber) === cellNum && item.dataset.clueDirection === direction) {
                item.classList.add('active-clue');
                item.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
            }
        });
    }

    function updateActiveClue() {
        const labelEl = document.getElementById('active-clue-label');
        const textEl = document.getElementById('active-clue-text');

        // Find clue for current word
        let startRow = activeRow, startCol = activeCol;
        if (direction === 'Across') {
            while (startCol > 0 && solutionGrid[startRow][startCol - 1] !== '#') startCol--;
        } else {
            while (startRow > 0 && solutionGrid[startRow - 1][startCol] !== '#') startRow--;
        }

        const cellNum = cellNumberMap[startRow] && cellNumberMap[startRow][startCol];

        // Find matching clue item
        for (const item of clueItems) {
            if (parseInt(item.dataset.clueNumber) === cellNum && item.dataset.clueDirection === direction) {
                labelEl.textContent = `Current Clue — ${cellNum} ${direction}`;
                textEl.textContent = item.querySelector('p').textContent;
                return;
            }
        }

        labelEl.textContent = `${direction}`;
        textEl.textContent = 'Navigate to a numbered cell.';
    }

    // ─── Keyboard Input ───
    function bindEvents() {
        document.addEventListener('keydown', onKeyDown);

        // Clue item clicks
        clueItems.forEach(item => {
            item.addEventListener('click', () => {
                const row = parseInt(item.dataset.startRow);
                const col = parseInt(item.dataset.startCol);
                direction = item.dataset.clueDirection;
                activeRow = row;
                activeCol = col;
                highlightWord();
                updateActiveClue();
            });
        });

        // Check Solve button
        document.getElementById('btn-check-solve').addEventListener('click', checkSolve);

        // Help modal
        document.getElementById('btn-help').addEventListener('click', () => {
            document.getElementById('help-modal').classList.remove('hidden');
        });
        document.getElementById('help-close').addEventListener('click', () => {
            document.getElementById('help-modal').classList.add('hidden');
        });

        // Result overlay close
        document.getElementById('result-close').addEventListener('click', () => {
            document.getElementById('result-overlay').classList.add('hidden');
        });
    }

    function onKeyDown(e) {
        if (isSolved) return;
        if (activeRow < 0 || activeCol < 0) return;

        // Ignore if modal is open
        if (!document.getElementById('help-modal').classList.contains('hidden')) return;
        if (!document.getElementById('result-overlay').classList.contains('hidden')) return;

        const key = e.key;

        if (key === 'ArrowRight') {
            e.preventDefault();
            moveToNextCell(0, 1);
        } else if (key === 'ArrowLeft') {
            e.preventDefault();
            moveToNextCell(0, -1);
        } else if (key === 'ArrowDown') {
            e.preventDefault();
            moveToNextCell(1, 0);
        } else if (key === 'ArrowUp') {
            e.preventDefault();
            moveToNextCell(-1, 0);
        } else if (key === 'Backspace') {
            e.preventDefault();
            deleteLetter();
        } else if (key === 'Tab') {
            e.preventDefault();
            moveToNextWord();
        } else if (/^[a-zA-Z]$/.test(key)) {
            e.preventDefault();
            enterLetter(key.toUpperCase());
        }
    }

    function enterLetter(letter) {
        userGrid[activeRow][activeCol] = letter;
        const textEl = cellElements[activeRow][activeCol].querySelector('.cell-text');
        if (textEl) textEl.textContent = letter;

        // Remove any error styling
        cellElements[activeRow][activeCol].classList.remove('incorrect');

        updateCompletion();
        advanceCursor();
    }

    function deleteLetter() {
        if (userGrid[activeRow][activeCol]) {
            userGrid[activeRow][activeCol] = '';
            const textEl = cellElements[activeRow][activeCol].querySelector('.cell-text');
            if (textEl) textEl.textContent = '';
            cellElements[activeRow][activeCol].classList.remove('incorrect');
        } else {
            // Move back
            retreatCursor();
            if (activeRow >= 0 && activeCol >= 0) {
                userGrid[activeRow][activeCol] = '';
                const textEl = cellElements[activeRow][activeCol].querySelector('.cell-text');
                if (textEl) textEl.textContent = '';
                cellElements[activeRow][activeCol].classList.remove('incorrect');
            }
        }
        updateCompletion();
        highlightWord();
    }

    function advanceCursor() {
        if (direction === 'Across') {
            moveToNextCell(0, 1);
        } else {
            moveToNextCell(1, 0);
        }
    }

    function retreatCursor() {
        if (direction === 'Across') {
            moveToNextCell(0, -1);
        } else {
            moveToNextCell(-1, 0);
        }
    }

    function moveToNextCell(dr, dc) {
        let r = activeRow + dr;
        let c = activeCol + dc;

        // Skip black cells
        while (r >= 0 && r < gridSize && c >= 0 && c < gridSize) {
            if (solutionGrid[r][c] !== '#') {
                activeRow = r;
                activeCol = c;
                highlightWord();
                updateActiveClue();
                return;
            }
            r += dr;
            c += dc;
        }
    }

    function moveToNextWord() {
        // Find the next clue in sequence
        const currentClueItemIndex = Array.from(clueItems).findIndex(item =>
            item.classList.contains('active-clue'));

        let nextIndex = (currentClueItemIndex + 1) % clueItems.length;
        const nextClue = clueItems[nextIndex];

        direction = nextClue.dataset.clueDirection;
        activeRow = parseInt(nextClue.dataset.startRow);
        activeCol = parseInt(nextClue.dataset.startCol);
        highlightWord();
        updateActiveClue();
    }

    // ─── Timer ───
    function startTimer() {
        timerInterval = setInterval(() => {
            if (isSolved) return;
            timerSeconds++;
            const mins = Math.floor(timerSeconds / 60).toString().padStart(2, '0');
            const secs = (timerSeconds % 60).toString().padStart(2, '0');
            document.getElementById('timer-display').textContent = `${mins}:${secs}`;
        }, 1000);
    }

    // ─── Completion ───
    function updateCompletion() {
        let filled = 0;
        let total = 0;
        for (let r = 0; r < gridSize; r++) {
            for (let c = 0; c < gridSize; c++) {
                if (solutionGrid[r][c] !== '#') {
                    total++;
                    if (userGrid[r][c]) filled++;
                }
            }
        }
        const pct = total > 0 ? Math.round((filled / total) * 100) : 0;
        document.getElementById('completion-display').textContent = `${pct}%`;
    }

    // ─── Check Solve ───
    async function checkSolve() {
        if (isSolved) return;

        // Build user grid to send
        const gridToSend = [];
        for (let r = 0; r < gridSize; r++) {
            gridToSend[r] = [];
            for (let c = 0; c < gridSize; c++) {
                gridToSend[r][c] = userGrid[r][c] || '';
            }
        }

        try {
            const response = await fetch('/Home/CheckSolve', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ puzzleId: puzzleId, userGrid: gridToSend })
            });
            const data = await response.json();

            if (data.success) {
                isSolved = true;
                showResult(true, data.solveCount);
                // Mark all cells as correct
                for (let r = 0; r < gridSize; r++) {
                    for (let c = 0; c < gridSize; c++) {
                        if (solutionGrid[r][c] !== '#') {
                            cellElements[r][c].classList.add('correct');
                        }
                    }
                }
            } else {
                // Highlight incorrect cells
                // First clear previous errors
                for (let r = 0; r < gridSize; r++) {
                    for (let c = 0; c < gridSize; c++) {
                        cellElements[r][c].classList.remove('incorrect');
                    }
                }
                if (data.incorrectCells) {
                    data.incorrectCells.forEach(([r, c]) => {
                        cellElements[r][c].classList.add('incorrect');
                    });
                }
                showResult(false);
            }
        } catch (err) {
            console.error('Check solve error:', err);
        }
    }

    function showResult(success, solveCount) {
        const overlay = document.getElementById('result-overlay');
        const icon = document.getElementById('result-icon');
        const title = document.getElementById('result-title');
        const message = document.getElementById('result-message');

        if (success) {
            icon.textContent = 'celebration';
            icon.style.color = '#705d00';
            title.textContent = 'Brilliant!';
            message.textContent = `You solved today's puzzle in ${document.getElementById('timer-display').textContent}. Global solvers: ${solveCount?.toLocaleString() ?? '—'}`;
        } else {
            icon.textContent = 'close';
            icon.style.color = '#9e422c';
            title.textContent = 'Not quite right';
            message.textContent = 'Incorrect cells have been highlighted in red. Keep trying!';
        }

        overlay.classList.remove('hidden');
    }

    // ─── SignalR ───
    function initSignalR() {
        if (typeof signalR === 'undefined') return;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/puzzlehub')
            .withAutomaticReconnect()
            .build();

        connection.on('UpdateSolveCount', (count) => {
            document.getElementById('global-solve-count').textContent =
                `Global Solve Count: ${count.toLocaleString()}`;
        });

        connection.start().catch(err => console.log('SignalR error:', err));
    }

    // ─── Boot ───
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
