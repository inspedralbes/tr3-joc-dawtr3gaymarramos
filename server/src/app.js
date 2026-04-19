// 1. Importacions principals
const express = require('express');
const dotenv = require('dotenv');
const cors = require('cors');
const connectDB = require('./config/database');

// 2. Importació de rutes
const userRoutes = require('./routes/userRoutes');
const gameRoutes = require('./routes/gameRoutes');

// 3. Configuració d'entorn i connexió a BD
dotenv.config();
connectDB();

// 4. Inicialitzar l'aplicació Express
const app = express();

// 5. Middlewares bàsics
app.use(cors()); // Permet connexions externes (Unity)
app.use(express.json()); // Permet llegir JSON al body de les peticions

// 6. Definició de Rutes de l'API
// Esto le dice al servidor: "Cualquier cosa que venga con /api/users, mándala al archivo de rutas"
app.use('/api/users', userRoutes);
app.use('/api/games', gameRoutes);

// Ruta de prova (Health Check)
app.get('/api/status', (req, res) => {
    res.json({ status: 'ok', message: 'Servidor del joc funcionant correctament' });
});
// ... (tus pasos 1 al 6 se quedan igual)

// 7. Configuración de WebSockets (Socket.io)
const http = require('http').Server(app);
const io = require('socket.io')(http, {
    cors: {
        origin: "*", // Permite que Unity se conecte desde cualquier sitio
        methods: ["GET", "POST"]
    }
});

// Lógica de comunicación multijugador
io.on('connection', (socket) => {
    console.log('🎮 Jugador conectado al socket:', socket.id);

    // Unirse a una sala (Punto 3.1.3 del enunciado)
    socket.on('joinRoom', (roomName) => {
        socket.join(roomName);
        console.log(`👤 Jugador unido a la sala: ${roomName}`);
        
        // Avisar a los demás en la sala
        socket.to(roomName).emit('playerJoined', { id: socket.id });
    });

    // Sincronización básica de posición (Punto 3.1.4 y 3.2.2)
    socket.on('move', (data) => {
        // data debe traer: { room: "nombre", x: 0, y: 0 }
        socket.to(data.room).emit('updateRemotePlayer', {
            id: socket.id,
            x: data.x,
            y: data.y
        });
    });

    // Sincronización de eventos (Muerte / Game Over)
    socket.on('playerStatus', (data) => {
        // Si un jugador muere, avisamos a la sala
        socket.to(data.room).emit('onPlayerStatusChanged', data);
    });

    socket.on('disconnect', () => {
        console.log('Jugador desconectado');
    });
});

// 8. Arrancar el servidor usando 'http' en lugar de 'app'
const PORT = process.env.PORT || 3000;
http.listen(PORT, () => {
    console.log(`Servidor multijugador corriendo en el puerto ${PORT}`);
});