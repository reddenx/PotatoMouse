module.exports = {
    devServer: {
        // https: true
    },
    css: {
        extract: false,
    },
    configureWebpack: {
        optimization: {
            splitChunks: false
        }
    }
}