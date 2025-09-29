const fs = require('fs');
const path = require('path');

const ENVIRONMENT_FILE = path.join(__dirname, 'src/environments/environment.ts');
const PROD_ENVIRONMENT_FILE = path.join(__dirname, 'src/environments/environment.ts');

/**
 * Load environment variables from .env file
 * @param {string} envFilePath - Path to the .env file
 * @returns {Object} Environment variables object
 */
function loadEnvFile(envFilePath) {
  const envVars = {};

  if (fs.existsSync(envFilePath)) {
    console.log(`📄 Loading environment from: ${envFilePath}`);
    const envContent = fs.readFileSync(envFilePath, 'utf8');

    envContent.split('\n').forEach(line => {
      if (line.trim() && !line.trim().startsWith('#')) {
        const [key, ...valueParts] = line.split('=');
        if (key && valueParts.length > 0) {
          const value = valueParts.join('=').trim();
          envVars[key.trim()] = value.replace(/^["']|["']$/g, '');
        }
      }
    });
  } else {
    console.log(`⚠️  Environment file not found: ${envFilePath}`);
  }

  return envVars;
}


const localEnvPath = path.join(__dirname, '.env');

let envVars = {};
if (fs.existsSync(localEnvPath)) {
  envVars = loadEnvFile(localEnvPath);
} else {
  console.log('⚠️  No .env file found, using process.env only');
}

const API_URL = envVars.API_URL || process.env.API_URL;

console.log('🔧 Injecting environment variables...');
console.log(`📡 API_URL: ${API_URL}`);
console.log(`📁 Checked paths: ${localEnvPath}`);

/**
 * Generate environment file content
 * @param {boolean} production - Whether this is for production
 * @returns {string} The environment file content
 */
function generateEnvironmentContent() {
  return `export const environment = {
  production: ${true},
  apiUrl: '${API_URL}',
};
`;
}

/**
 * Write environment file
 * @param {string} filePath - Path to the environment file
 * @param {boolean} production - Whether this is for production
 */
function writeEnvironmentFile(filePath) {
  try {
    const content = generateEnvironmentContent();

    const dir = path.dirname(filePath);
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
    }

    fs.writeFileSync(filePath, content, 'utf8');
    console.log(`✅ Successfully updated: ${filePath}`);
  } catch (error) {
    console.error(`❌ Error writing ${filePath}:`, error.message);
    process.exit(1);
  }
}

try {
  writeEnvironmentFile(PROD_ENVIRONMENT_FILE, true);

  console.log('🎉 Environment injection completed successfully!');
} catch (error) {
  console.error('💥 Environment injection failed:', error.message);
  process.exit(1);
}
