const mongoose = require('mongoose');

const connectDB = async () => {
    try {
        // Intentem connectar a la base de dades utilitzant la URL de l'arxiu .env
        const conn = await mongoose.connect(process.env.MONGO_URI);
        console.log(`Connectat a MongoDB: ${conn.connection.host}`);
    } catch (error) {
        console.error(`Error de connexió a MongoDB: ${error.message}`);
        process.exit(1); // Aturem el servidor si no hi ha base de dades
    }
};

module.exports = connectDB;