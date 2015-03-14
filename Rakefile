task :default, :deploy

desc "Deploy to GitHub"
task :deploy do
  puts '>> Checking for unstaged changes'
  continue = system("git diff --exit-code")

  if !continue
    puts ">> Unstaged changes found. Exiting"
    exit
  end

  puts ">> git branch deploy"
  `git branch deploy`
  
  branches=`git branch -v`
  puts branches
  
  puts ">> git filter-branch"
  `git filter-branch --index-filter 'git rm --cached --ignore-unmatch Encryption.cs' -f deploy`
  
  puts ">> git push --force origin deploy"
  `git push --force -u origin deploy:master`
  
  puts ">> git branch -D deploy"
  `git branch -D deploy`
  
  branches=`git branch -v`
  puts branches
end