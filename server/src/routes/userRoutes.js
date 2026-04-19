const express = require('express');
const router = express.Router();
const userController = require('../controllers/UserController');

const User = require('../models/User'); 

router.post('/register', userController.register);
router.post('/login', userController.login); 

router.post('/save-stats', async (req, res) => {
    try {
        const { userId, nightsReached, victory } = req.body;
        
        // Creamos el objeto de actualización
        let updateData = {
            $inc: { "stats.gamesPlayed": 1 }, // <--- USAMOS EL PUNTO PARA ENTRAR EN STATS
            $set: { lastNight: nightsReached } 
        };

        // Si ha ganado, también sumamos uno a gamesWon (que está en tu Schema)
        if (victory) {
            updateData.$inc["stats.gamesWon"] = 1;
        }

        const userUpdated = await User.findByIdAndUpdate(
            userId, 
            updateData,
            { new: true }
        );

        if (!userUpdated) {
            return res.status(404).json({ status: "error", message: "Usuario no encontrado" });
        }

        console.log(`Stats de ${userUpdated.username} actualizadas: Partidas: ${userUpdated.stats.gamesPlayed}`);
        res.status(200).json({ status: "success", data: userUpdated.stats });

    } catch (error) {
        console.error("Error en save-stats:", error);
        res.status(500).json({ status: "error", message: error.message });
    }
});

module.exports = router;