const mongoose = require('mongoose');

// Esquema per a gestionar les sales multijugador i l'estat de la partida
const gameSchema = new mongoose.Schema({
    roomCode: { 
        type: String, 
        required: true, 
        unique: true // Codi únic per entrar a la sala
    },
    hostId: { 
        type: mongoose.Schema.Types.ObjectId, 
        ref: 'User', 
        required: true // El creador de la partida
    },
    players: [{ 
        type: mongoose.Schema.Types.ObjectId, 
        ref: 'User' // Jugadors connectats
    }],
    status: { 
        type: String, 
        enum: ['waiting', 'playing', 'finished'], 
        default: 'waiting' 
    },
    maxPlayers: { 
        type: Number, 
        default: 4 
    },
    createdAt: { 
        type: Date, 
        default: Date.now 
    }
});

module.exports = mongoose.model('Game', gameSchema);