const bcrypt = require('bcrypt');

class UserService {
    constructor(userRepository) {
        this.userRepository = userRepository;
    }

    async register(username, password) {
        if (!username || !password) throw new Error("Falten dades");
        const existingUser = await this.userRepository.findByUsername(username);
        if (existingUser) throw new Error("L'usuari ja existeix");

        const salt = await bcrypt.genSalt(10);
        const hashedPassword = await bcrypt.hash(password, salt);

        return await this.userRepository.create({ 
            username, 
            password: hashedPassword 
        });
    }

    // --- AFEGIM AIXÒ ---
    async login(username, password) {
        if (!username || !password) throw new Error("Falten dades");

        const user = await this.userRepository.findByUsername(username);
        if (!user) throw new Error("L'usuari no existeix");

        // Comparem la contrasenya que envia Unity amb la xifrada de la DB
        const isMatch = await bcrypt.compare(password, user.password);
        if (!isMatch) throw new Error("Contrasenya incorrecta");

        return user; // Si tot és correcte, retornem l'usuari
    }
}

module.exports = UserService;