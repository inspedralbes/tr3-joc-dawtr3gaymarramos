const express = require('express');
const router = express.Router();
const gameController = require('../controllers/GameController');

// Ruta per crear una partida nova (POST /api/games/create)
router.post('/create', gameController.create);

// Ruta per unir-se a una partida existent (POST /api/games/join)
router.post('/join', gameController.join);

module.exports = router;