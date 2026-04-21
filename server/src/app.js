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
app.use('/api/users', userRoutes);
app.use('/api/games', gameRoutes);

// Ruta de prova (Health Check)
app.get('/api/status', (req, res) => {
    res.json({ status: 'ok', message: 'Servidor del joc funcionant correctament' });
});

// 7. Configuración de WebSockets (Socket.io)
const http = require('http').Server(app);
const io = require('socket.io')(http, {
    cors: {
        origin: "*", 
        methods: ["GET", "POST"]
    }
});

// Lógica de comunicación multijugador
io.on('connection', (socket) => {
    console.log('Jugador conectado:', socket.id);

    // Evento para entrar en la sala de espera (Lobby)
    socket.on('joinRoom', async (data) => {
        const { roomCode, username } = data;
        socket.join(roomCode);
        
        console.log(`Usuario ${username} se ha unido a la sala: ${roomCode}`);

        socket.username = username;
        socket.room = roomCode;

        const roomSockets = await io.in(roomCode).fetchSockets();
        const numPlayers = roomSockets.length;
        
        const players = [];
        for (const s of roomSockets) {
            if (s.username) {
                players.push(s.username);
            }
        }
        
        io.in(roomCode).emit('roomUpdated', {
            playersCount: numPlayers,
            lastJoined: username,
            players: players
        });
    });

    // Sincronización de movimiento durante la partida
    socket.on('move', (data) => {
        const room = data.room || socket.room;
        
        if (room) {
            const payload = { 
                id: socket.id, 
                pos: data.pos,
                username: socket.username,
                isHost: data.isHost
            };
            
            // Reenviamos a los otros jugadores
            socket.to(room).emit('playerMoved', payload);
        }
    });

    // NUEVO: Sincronización de enemigos (solo el Host envía esto)
    socket.on('enemyMove', (data) => {
        // Enviamos a TODOS (incluido el Host para confirmar o solo a otros)
        // Usamos io.emit para asegurar que llegue a todo el mundo
        io.emit('enemyUpdated', data);
    });

    // NUEVO: Sincronización de estado abatido
    socket.on('playerDowned', (data) => {
        const room = data.room || socket.room;
        if (room) {
            socket.to(room).emit('onPlayerDowned', data);
        } else {
            socket.broadcast.emit('onPlayerDowned', data);
        }
    });

    socket.on('playerRevived', (data) => {
        const room = data.room || socket.room;
        if (room) {
            socket.to(room).emit('onPlayerRevived', data);
        } else {
            socket.broadcast.emit('onPlayerRevived', data);
        }
    });

    // NUEVO: Sincronización de daño
    socket.on('playerDamage', (data) => {
        io.emit('onPlayerDamaged', data);
    });

    // Evento para cuando el Host le da a "Comenzar Partida"
    socket.on('startGame', (roomCode) => {
        io.in(roomCode).emit('onGameStarted');
    });

    // Evento para cuando el Cliente confirma que está listo
    socket.on('playerReady', (roomCode) => {
        socket.to(roomCode).emit('opponentReady');
    });

    socket.on('disconnect', () => {
        if(socket.room) {
            io.in(socket.room).emit('playerLeft', { username: socket.username });
        }
        console.log('Jugador desconectado');
    });
});

// 8. Arrancar el servidor
const PORT = process.env.PORT || 3000;
http.listen(PORT, () => {
    console.log(`Servidor escuchando en el puerto ${PORT}`);
});