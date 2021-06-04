node {
    stage('Clone repository') {
        git branch: 'main', credentialsId: 'github-app-IAmSilK', url: 'https://github.com/IAmSilK/Drop'
    }
    
    stage('Build image') {
        app = docker.build("drop-webapp")
    }
    
    stage('Push image') {
        docker.withRegistry('http://127.0.0.1:6000') {
            app.push("1.0.${env.BUILD_NUMBER}")
            app.push('latest')
        }
    }
    
    stage('Deploy container') {
        sh '''
            # Stop drop-webapp docker if it's running
            docker ps -q --filter "name=drop-webapp" | grep -q . && docker stop drop-webapp

            # Remove drop-webapp docker if it exists
            docker ps -a -q --filter "name=drop-webapp" | grep -q . && docker rm -fv drop-webapp

            # Create and start drop-webapp container
            docker run -d \
                -p 5000:80 \
                -e ASPNETCORE_ENVIRONMENT=Production \
                -e DOTNET_PRINT_TELEMETRY_MESSAGE=false \
                -v drop-webapp:/data \
                --name drop-webapp \
                drop-webapp:latest
        '''
    }
}