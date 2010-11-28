UnRarIt.Net
===========
Batch rar/zip/7z/split extraction tool in .Net

Features 
--------
* Support for RAR, ZIP and 7-ZIP
* Support for Split-Archives
* Support for nested archives
* Password "cracking" using a password list. Recently/often used passwords will be tried first to speed up the process.
* Smart directory management. Generate directories on the fly. Strip non-required directories.
* Auto-renaming and/or deletion of successfully processed archives.
* Per file/archive/session existing file handling.
* Optimized extraction libraries specific to the processor to gain a speed boost for password "cracking" and extraction
* Fully multi-threaded to take advantage of multi-core/-processor systems

Build
-----
1. Clone this repository and init submodule(s) (git submodule update --init)
2. Build 7zip
3. Build UnRarIt (See solution file)
4. Copy the 7zip DLLs to the corresponding Debug/Release folder
5. Build the Setup Project

License
-------
Written 2009 by Nils Maier - Copyrights are disclaimed - Public domain
You may use this program freely for any means and without any limitation.

GetOptNet and UnRar are copyrighted and available under free-to-use or completely free licenses.
7-Zip is copyrighted by Igor Pavlov and available under the GNU Lesser General Public License. See <http://7-zip.org>

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
