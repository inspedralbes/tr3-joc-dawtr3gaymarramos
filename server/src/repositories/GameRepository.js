const Game = require('../models/Game');

class GameRepository {
    // Crear una nova sala
    async create(gameData) {
        const game = new Game(gameData);
        return await game.save();
    }

    // Buscar una sala pel seu codi únic (ex: "X7B9")
    async findByCode(roomCode) {
        return await Game.findOne({ roomCode }).populate('players', 'username');
    }

    // Afegir un jugador a la llista de la sala
    async addPlayer(gameId, userId) {
        return await Game.findByIdAndUpdate(
            gameId,
            { $addToSet: { players: userId } }, // $addToSet evita duplicats
            { new: true }
        ).populate('players', 'username');
    }
}

module.exports = GameRepository;