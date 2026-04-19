class GameService {
    constructor(gameRepository) {
        this.gameRepository = gameRepository;
    }

    // Funció per crear una sala amb un codi aleatori
    async createGame(hostId) {
        // Generem un codi de 6 lletres/números aleatoris
        const roomCode = Math.random().toString(36).substring(2, 8).toUpperCase();
        
        return await this.gameRepository.create({
            roomCode,
            hostId,
            players: [hostId], // El host entra directament a la sala
            status: 'waiting'
        });
    }

    async joinGame(roomCode, userId) {
        const game = await this.gameRepository.findByCode(roomCode);
        
        if (!game) throw new Error("La sala no existeix");
        if (game.status !== 'waiting') throw new Error("La partida ja ha començat");
        if (game.players.length >= game.maxPlayers) throw new Error("La sala està plena");

        return await this.gameRepository.addPlayer(game._id, userId);
    }
}

module.exports = GameService;