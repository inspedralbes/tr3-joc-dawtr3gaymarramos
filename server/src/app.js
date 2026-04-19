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

// ... (Tus pasos 1 al 6 se quedan exactamente igual)

// 7. Configuración de WebSockets (Socket.io)
const http = require('http').Server(app);
const io = require('socket.io')(http, {
    cors: {
        origin: "*", 
        methods: ["GET", "POST"]
    }
});

// Lógica de comunicación multijugador (FUSIONADA Y LIMPIA)
io.on('connection', (socket) => {
    console.log('🎮 Jugador conectado:', socket.id);

    // Evento para entrar en la sala de espera (Lobby)
    socket.on('joinRoom', (data) => {
        const { roomCode, username } = data;
        socket.join(roomCode);
        
        console.log(`👤 ${username} se ha unido a la sala: ${roomCode}`);

        // Guardamos el nombre en el socket para saber quién es al desconectar
        socket.username = username;
        socket.room = roomCode;

        // Avisamos a TODOS en la sala (incluido el que entra) para actualizar la lista
        // Aquí enviamos la info para tu nueva escena de "Sala de Espera"
        const clients = io.sockets.adapter.rooms.get(roomCode);
        const numPlayers = clients ? clients.size : 0;

        io.in(roomCode).emit('roomUpdated', {
            playersCount: numPlayers,
            lastJoined: username
        });
    });

    // Sincronización de movimiento durante la partida
    socket.on('move', (data) => {
        // Enviamos a todos los de la sala menos al que emite
        socket.to(data.room).emit('playerMoved', { 
            id: socket.id, 
            pos: data.pos,
            username: socket.username 
        });
    });

    // Evento para cuando el Host le da a "Comenzar Partida"
    socket.on('startGame', (roomCode) => {
        io.in(roomCode).emit('onGameStarted');
    });

    socket.on('disconnect', () => {
        if(socket.room) {
            io.in(socket.room).emit('playerLeft', { username: socket.username });
        }
        console.log('Jugador desconectat');
    });
});

// 8. Arrancar el servidor
const PORT = process.env.PORT || 3000;
http.listen(PORT, () => {
    console.log(`Servidor escoltant el port ${PORT}`);
});