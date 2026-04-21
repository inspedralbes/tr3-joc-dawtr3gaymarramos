const User = require('../models/User');

class UserRepository {
    // Busca un usuari pel seu nom (alias)
    async findByUsername(username) {
        return await User.findOne({ username: username });
    }

    // Guarda un nou usuari a la base de dades
    async create(userData) {
        const user = new User(userData);
        return await user.save();
    }
}

module.exports = UserRepository;