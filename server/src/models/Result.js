const mongoose = require('mongoose');

// Esquema per guardar l'historial del Game Over (Individual i Multi)
const resultSchema = new mongoose.Schema({
    userId: { 
        type: mongoose.Schema.Types.ObjectId, 
        ref: 'User', 
        required: true 
    },
    survived: { 
        type: Boolean, 
        required: true // True si arriba a les 6 AM
    },
    livesRemaining: { 
        type: Number, 
        default: 0 
    },
    timeSurvived: { 
        type: String, 
        required: true // Exemple: "05:12 AM"
    },
    createdAt: { 
        type: Date, 
        default: Date.now 
    }
});

module.exports = mongoose.model('Result', resultSchema);