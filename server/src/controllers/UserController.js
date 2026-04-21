const UserRepository = require('../repositories/UserRepository');
const UserService = require('../services/UserService');

const userRepository = new UserRepository();
const userService = new UserService(userRepository);

class UserController {
    async register(req, res) {
        try {
            const { username, password } = req.body;
            const user = await userService.register(username, password);
            res.status(201).json({ status: "success", data: { username: user.username } });
        } catch (error) {
            res.status(400).json({ status: "error", message: error.message });
        }
    }

    // --- AFEGIM AIXÒ ---
    async login(req, res) {
        try {
            const { username, password } = req.body;
            const user = await userService.login(username, password);
            
            res.status(200).json({
                status: "success",
                message: "Login correcte",
                data: { id: user._id, username: user.username }
            });
        } catch (error) {
            res.status(401).json({ status: "error", message: error.message });
        }
    }
}

module.exports = new UserController();