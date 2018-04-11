var path = require("path");
var webpack = require("webpack");
let fableUtils = require("fable-utils");

function resolve(filePath) {
  return path.join(__dirname, filePath);
}

var babelOptions = fableUtils.resolveBabelOptions({
  presets: [
    [
      "env",
      {
        targets: {
          browsers: ["last 2 versions"]
        },
        modules: false
      }
    ]
  ],
  plugins: ["transform-runtime"]
});

var isProduction = process.argv.indexOf("-p") >= 0;
var suavePort = process.env.SUAVE_FABLE_PORT || "8085";
console.log(
  "Bundling for " + (isProduction ? "production" : "development") + "..."
);

module.exports = {
  devtool: "source-map",
  entry: resolve("./src/Client/Client.fsproj"),
  output: {
    path: resolve("./public"),
    publicPath: "/public",
    filename: "bundle.js"
  },
  resolve: {
    modules: ["node_modules", resolve("./node_modules/")]
  },
  devServer: {
    proxy: {
      "/api/*": {
        target: "http://localhost:" + suavePort,
        changeOrigin: true
      }
    },
    contentBase: resolve("./public"),
    hot: true,
    inline: true,
    historyApiFallback: true
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: {
          loader: "fable-loader",
          options: {
            babel: babelOptions,
            define: isProduction ? [] : ["DEBUG"]
          }
        }
      },
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: "babel-loader",
          options: babelOptions
        }
      },
      {
        test: /\.sass$/,
        use: ["style-loader", "css-loader", "sass-loader"]
      }
    ]
  },
  plugins: isProduction
    ? []
    : [
        new webpack.HotModuleReplacementPlugin(),
        new webpack.NamedModulesPlugin()
      ]
};
