const GameRepository = require('../repositories/GameRepository');
const GameService = require('../services/GameService');

const gameRepository = new GameRepository();
const gameService = new GameService(gameRepository);

class GameController {
    async create(req, res) {
        try {
            const { hostId } = req.body;
            const game = await gameService.createGame(hostId);
            res.status(201).json({ status: "success", data: game });
        } catch (error) {
            res.status(400).json({ status: "error", message: error.message });
        }
    }

    async join(req, res) {
        try {
            const { roomCode, userId } = req.body;
            const game = await gameService.joinGame(roomCode, userId);
            res.status(200).json({ status: "success", data: game });
        } catch (error) {
            res.status(400).json({ status: "error", message: error.message });
        }
    }
}

module.exports = new GameController();